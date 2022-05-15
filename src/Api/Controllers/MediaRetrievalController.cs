using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auricular.DataAccess;
using Auricular.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    [Authorize]
    public class MediaRetrievalController : ControllerBase {
        public MediaRetrievalController(IMediaLibrary index, IMemoryCache cache) {
            Index = index;
            Cache = cache;
        }

        public IMediaLibrary Index { get; }
        public IMemoryCache Cache { get; }

        [HttpGet]
        [Route("stream")]
        [Route("stream.view")]
        public FileStreamResult StreamFile(int id) {
            MediaLibraryEntry entry = Index.GetFile(id);

            // Temporary!
            if (entry == null) {
                entry = Index.GetChildEntries(id)
                    .Where(e => !e.IsFolder)
                    .OrderBy(e => e.TrackNumber)
                    .FirstOrDefault();
            }

            Index.UpdateLastPlayed(id);

            // howler.js HTML5 playback mode relies on range-requests
            // TODO: Proper MIME type support
            return File(entry.OpenReadStream(), "audio/mpeg", enableRangeProcessing: true);
        }

        // TODO: Cache thumbnails after generation
        // TODO: Etag cache control
        [HttpGet]
        [Route("getCoverArt")]
        [Route("getCoverArt.view")]
        public async Task<ActionResult> GetCoverArt(int id) {
            string cacheKey = $"CoverArt_{id.ToString(CultureInfo.InvariantCulture)}";
            if (Cache.TryGetValue<byte[]>(cacheKey, out byte[] cachedItem)) {
                return File(cachedItem, "image/jpeg");
            }

            MediaLibraryEntry entry = Index.GetEntry(id);

            bool originalIsFolder = entry.IsFolder;

            if (!originalIsFolder) {
                using (var id3File = TagLib.File.Create(entry.Path)) {
                    TagLib.IPicture image = id3File.Tag.Pictures
                        .OrderByDescending(i => i.Type == TagLib.PictureType.FrontCover)
                        .FirstOrDefault();

                    if (image != null) {
                        byte[] bytes = await GenerateThumbnail(image.Data.Data);
                        Cache.Set(cacheKey, bytes);

                        return new FileContentResult(bytes, "image/jpeg");
                    }
                }

                // Nothing in the MP3 file, so let's try to get an image from the parent folder
                entry = Index.GetFolder(entry.ParentId);
            }

            Debug.Assert(entry.IsFolder);

            FileInfo file = new DirectoryInfo(entry.Path)
                .EnumerateFiles("*.jpg")
                .FirstOrDefault();

            if (file == null) {
                // If the request was for a folder and there is no image file,
                // try to find an image in the MP3 files
                if (originalIsFolder) {
                    entry = Index.GetChildFiles(entry.Id)
                        .FirstOrDefault();

                    using (var id3File = TagLib.File.Create(entry.Path)) {
                        TagLib.IPicture image = id3File.Tag.Pictures
                            .OrderByDescending(i => i.Type == TagLib.PictureType.FrontCover)
                            .FirstOrDefault();

                        if (image != null) {
                            byte[] bytes = await GenerateThumbnail(image.Data.Data);
                            Cache.Set(cacheKey, bytes);

                            return new FileContentResult(bytes, "image/jpeg");
                        }
                    }

                    // Nothing in the MP3 file either
                }

                return new FileContentResult(Array.Empty<byte>(), "image/jpeg");
            }

            byte[] bytes1 = await GenerateThumbnail(System.IO.File.ReadAllBytes(file.FullName));
            Cache.Set(cacheKey, bytes1);

            return new FileContentResult(bytes1, "image/jpeg");
        }

        private async Task<byte[]> GenerateThumbnail(byte[] data) {
            using var stream = new MemoryStream();
            await GenerateThumbnail(data, stream);

            return stream.ToArray();
        }

        private async Task GenerateThumbnail(byte[] data, Stream writeToStream) {
            using var image = Image.Load(data);
            image.Mutate(i => i.Resize(new ResizeOptions() {
                Size = new Size(256),
                Mode = ResizeMode.Max,
            }));

            await image.SaveAsJpegAsync(writeToStream);
        }
    }
}
