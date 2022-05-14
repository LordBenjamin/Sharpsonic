using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auricular.DataAccess;
using Auricular.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    [Authorize]
    public class MediaRetrievalController : ControllerBase {
        public MediaRetrievalController(IMediaLibrary index) {
            Index = index;
        }

        public IMediaLibrary Index { get; }


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

            return new FileStreamResult(entry.OpenReadStream(), "audio/mp3");
        }

        // TODO: Cache thumbnails after generation
        // TODO: Etag cache control
        [HttpGet]
        [Route("getCoverArt")]
        [Route("getCoverArt.view")]
        public async Task<ActionResult> GetCoverArt(int id) {
            MediaLibraryEntry entry = Index.GetEntry(id);

            bool originalIsFolder = entry.IsFolder;

            if (!originalIsFolder) {
                using (var id3File = TagLib.File.Create(entry.Path)) {
                    TagLib.IPicture image = id3File.Tag.Pictures
                        .OrderByDescending(i => i.Type == TagLib.PictureType.FrontCover)
                        .FirstOrDefault();

                    if (image != null) {
                        byte[] bytes = await GenerateThumbnail(image.Data.Data);
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
                            return new FileContentResult(bytes, "image/jpeg");
                        }
                    }

                    // Nothing in the MP3 file either
                }

                return new FileContentResult(Array.Empty<byte>(), "image/jpeg");
            }

            byte[] bytes1 = await GenerateThumbnail(System.IO.File.ReadAllBytes(file.FullName));
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
