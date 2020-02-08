using Api.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Api.Controllers {
    [Route("rest")]
    [ApiController]
    [Produces("application/xml")]
    public class BrowsingController : ControllerBase {
        private readonly string musicPath;

        public MediaIndex Index { get; }

        public BrowsingController(IOptions<ApplicationSettings> appSettings, MediaIndex index) {
            musicPath = appSettings.Value.MusicSourceDirectory;
            Index = index;
        }

        [HttpGet]
        [Route("getMusicFolders")]
        [Route("getMusicFolders.view")]
        public ActionResult<Response> GetMusicFolders() {

            return new Response {
                Item = new MusicFolders {
                    musicFolder = Index.Entries
                        .Where(i => i.ParentId == -1)
                        .Where(i => i.IsFolder)
                        .Select(i => new MusicFolder {
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
        public ActionResult<Response> GetMusicDirectory(int id) {
            MediaIndexEntry dir = Index.Entries
                .Where(i => i.Id == id)
                .SingleOrDefault();

            MediaIndexEntry rootDir = dir;
            while (rootDir.ParentId >= 0) {
                rootDir = Index.Entries
                    .Where(i => i.Id == rootDir.ParentId)
                    .SingleOrDefault();
            }


            return new Response {
                Item = new Directory {
                    id = dir.Id.ToString(CultureInfo.InvariantCulture),
                    name = dir.Name,
                    parent = dir.ParentId.ToString(CultureInfo.InvariantCulture),
                    child = Index.Entries
                        .Where(i => i.ParentId == id)
                        .Select(i => new Child {
                            id = i.Id.ToString(CultureInfo.InvariantCulture),
                            parent = i.ParentId.ToString(CultureInfo.InvariantCulture),
                            artist = i.Artist,
                            title = i.Name,
                            album = dir.Name,
                            isDir = i.IsFolder,
                            track = i.TrackNumber ?? 0,
                            trackSpecified = i.TrackNumber.HasValue,
                            coverArt =  i.Id.ToString(CultureInfo.InvariantCulture),
                            duration = i.Duration.HasValue ? (int)Math.Ceiling(i.Duration.Value.TotalSeconds) : 0,
                            durationSpecified = i.Duration.HasValue,
                            path = Path.GetRelativePath(rootDir.Path, i.Path),
                        })
                        .OrderBy(i => i.trackSpecified)
                        .ThenBy(i => i.album)
                        .ThenBy(i => i.track)
                        .ThenBy(i => i.title)
                        .ToArray(),
                },
                ItemElementName = ItemChoiceType.directory,
                version = "1.14.0",
            };
        }

        [HttpGet]
        [Route("getIndexes")]
        [Route("getIndexes.view")]
        public ActionResult<Response> GetIndexes(int? musicFolderId, long ifModifiedSince) {

            return new Response {
                Item = new Indexes {
                    index = SearchIndexes(musicFolderId, ifModifiedSince)
                },
                ItemElementName = ItemChoiceType.indexes,
            };
        }

        private Index[] SearchIndexes(int? musicFolderId, long ifModifiedSince) {
            IEnumerable<MediaIndexEntry> entries = Index.Entries
                .Where(i => i.IsFolder && i.ParentId == 0);

            if (musicFolderId.HasValue) {
                entries = entries.Where(i => i.ParentId == musicFolderId.Value);
            }

            return entries
                .GroupBy(i => i.Name.ToUpper().Substring(0, 1))
                .Select(g => new Index {
                    artist = g
                        .Select(i => new Artist {
                            id = i.Id.ToString(),
                            name = i.Name,
                        })
                        .ToArray(),
                    name = g.Key,
                })
                .OrderBy(i => i.name)
                .ToArray();
        }
    }
}
