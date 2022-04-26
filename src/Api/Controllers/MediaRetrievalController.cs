using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Auricular.DataAccess;
using Auricular.DataAccess.Entities;
using Auricular.DataAccess.InMemory;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
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
                entry = Index.GetChildEntries(id).FirstOrDefault();
            }

            Index.UpdateLastPlayed(id);

            return new FileStreamResult(entry.OpenReadStream(), "audio/mp3");
        }

        [HttpGet]
        [Route("getCoverArt")]
        [Route("getCoverArt.view")]
        public ActionResult GetCoverArt(int id) {
            MediaLibraryEntry entry = Index.GetEntry(id);

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
