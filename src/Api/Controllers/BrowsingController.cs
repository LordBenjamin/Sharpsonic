using Api.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Api.Controllers
{
    [Route("rest")]
    [ApiController]
    [Produces("application/xml")]
    public class BrowsingController : ControllerBase
    {
        private readonly string musicPath;

        public MediaIndex Index { get; }

        public BrowsingController(IOptions<ApplicationSettings> appSettings, MediaIndex index)
        {
            musicPath = appSettings.Value.MusicSourceDirectory;
            Index = index;
        }

        [HttpGet]
        [Route("getMusicFolders")]
        [Route("getMusicFolders.view")]
        public ActionResult<Response> GetMusicFolders()
        {

            return new Response
            {
                Item = new MusicFolders
                {
                    musicFolder = Index.Entries
                        .Where(i => i.ParentId == -1)
                        .Where(i => i.IsFolder)
                        .Select(i => new MusicFolder
                        {
                            id = i.Id,
                            name = i.Name,
                        })
                        .ToArray(),
                },
                ItemElementName = ItemChoiceType.musicFolders,
            };
        }

        [HttpGet]
        [Route("getMusicDirectory")]
        [Route("getMusicDirectory.view")]
        public ActionResult<Response> GetMusicDirectory(int id)
        {
            MediaIndexEntry dir = Index.Entries
                .Where(i => i.Id == id)
                .SingleOrDefault();

            return new Response
            {
                Item = new Directory
                {
                    name = dir.Name,
                    child = Index.Entries
                        .Where(i => i.ParentId == id)
                        .Select(i => new Child
                        {
                            id = i.Id.ToString(CultureInfo.InvariantCulture),
                            parent = i.ParentId.ToString(CultureInfo.InvariantCulture),
                            title = i.Name,
                            album = dir.Name,
                            isDir = i.IsFolder,
                            coverArt = (i.IsFolder ? i.Id : i.ParentId).ToString(CultureInfo.InvariantCulture),
                        })
                        .ToArray(),
                },
                ItemElementName = ItemChoiceType.directory,
            };
        }

        [HttpGet]
        [Route("getIndexes")]
        [Route("getIndexes.view")]
        public ActionResult<Response> GetIndexes(int? musicFolderId, long ifModifiedSince)
        {

            return new Response
            {
                Item = new Indexes
                {
                    index = SearchIndexes(musicFolderId, ifModifiedSince)
                },
                ItemElementName = ItemChoiceType.indexes,
            };
        }

        private Index[] SearchIndexes(int? musicFolderId, long ifModifiedSince)
        {
            IEnumerable<MediaIndexEntry> entries = Index.Entries
                .Where(i => i.IsFolder && i.ParentId == 0);

            if (musicFolderId.HasValue)
            {
                entries = entries.Where(i => i.ParentId == musicFolderId.Value);
            }

            return entries
                .GroupBy(i => i.Name.ToUpper().Substring(0, 1))
                .Select(g => new Index
                {
                    artist =g
                        .Select(i => new Artist {
                            id = i.Id.ToString(),
                            name = i.Name,
                        })
                        .ToArray(),
                    name = g.Key,
                })
                .ToArray();
        }
    }
}
