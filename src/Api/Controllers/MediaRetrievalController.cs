using Api.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Api.Controllers
{
    [Route("rest")]
    [ApiController]
    [Produces("application/xml")]
    public class MediaRetrievalController : ControllerBase
    {
        private readonly string musicPath;

        public MediaIndex Index { get; }

        public MediaRetrievalController(IOptions<ApplicationSettings> appSettings, MediaIndex index)
        {
            musicPath = appSettings.Value.MusicSourceDirectory;
            Index = index;
        }

        [HttpGet]
        [Route("stream")]
        [Route("stream.view")]
        public FileStreamResult StreamFile(int id)
        {
            MediaIndexEntry entry = Index.Entries
                .Where(i => i.Id == id)
                .FirstOrDefault();

            return new FileStreamResult(entry.OpenReadStream(), "audio/mp3");
        }

        [HttpGet]
        [Route("getCoverArt")]
        [Route("getCoverArt.view")]
        public ActionResult GetCoverArt(int id)
        {
            MediaIndexEntry entry = Index.Entries
                .Where(i => i.Id == id || (i.ParentId == id && !i.IsFolder))
                .OrderByDescending(i => i.IsFolder) // Prefer folders for now
                .FirstOrDefault();

            Debug.Assert(entry.IsFolder);

            FileInfo file = new DirectoryInfo(entry.Path)
                .EnumerateFiles("*.jpg")
                .FirstOrDefault();

            if(file == null)
            {
                return new FileContentResult(Array.Empty<byte>(), "image/jpeg");
            }

            return new FileStreamResult(file.OpenRead(), "image/jpeg");
        }
    }
}
