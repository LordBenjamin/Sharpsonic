using Sharpsonic.Api.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sharpsonic.Api.Settings;

namespace Sharpsonic.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    public class MediaRetrievalController : ControllerBase {
        public MediaIndex Index { get; }

        public MediaRetrievalController(MediaIndex index) {
            Index = index;
        }

        [HttpGet]
        [Route("stream")]
        [Route("stream.view")]
        public FileStreamResult StreamFile(int id) {
            MediaIndexEntry entry = Index.Entries
                .Where(i => i.Id == id)
                .FirstOrDefault();

            return new FileStreamResult(entry.OpenReadStream(), "audio/mp3");
        }

        [HttpGet]
        [Route("getCoverArt")]
        [Route("getCoverArt.view")]
        public ActionResult GetCoverArt(int id) {
            MediaIndexEntry entry = Index.Entries
                .Where(i => i.Id == id)
                .FirstOrDefault();

            bool originalIsFolder = entry.IsFolder;

            if (!originalIsFolder) {
                using (var id3File = TagLib.File.Create(entry.Path)) {
                    TagLib.IPicture image = id3File.Tag.Pictures
                        .OrderByDescending(i => i.Type == TagLib.PictureType.FrontCover)
                        .FirstOrDefault();

                    if (image != null) {
                        return new FileContentResult(image.Data.Data, image.MimeType);
                    }
                }

                // Nothing in the MP3 file, so let's try to get an image from the parent folder
                entry = Index.Entries
                    .Where(i => i.Id == entry.ParentId)
                    .FirstOrDefault();
            }

            Debug.Assert(entry.IsFolder);

            FileInfo file = new DirectoryInfo(entry.Path)
                .EnumerateFiles("*.jpg")
                .FirstOrDefault();

            if (file == null) {
                // If the request was for a folder and there is no image file,
                // try to find an image in the MP3 files
                if (originalIsFolder) {
                    entry = Index.Entries
                        .Where(i => i.ParentId == entry.Id)
                        .FirstOrDefault();

                    using (var id3File = TagLib.File.Create(entry.Path)) {
                        TagLib.IPicture image = id3File.Tag.Pictures
                            .OrderByDescending(i => i.Type == TagLib.PictureType.FrontCover)
                            .FirstOrDefault();

                        if (image != null) {
                            return new FileContentResult(image.Data.Data, image.MimeType);
                        }
                    }

                    // Nothing in the MP3 file either
                }

                return new FileContentResult(Array.Empty<byte>(), "image/jpeg");
            }

            return new FileStreamResult(file.OpenRead(), "image/jpeg");
        }
    }
}
