using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Auricular.Api.DataTransfer;
using Auricular.DataAccess;
using Auricular.DataAccess.Entities;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    public class BrowsingController : ControllerBase {

        public BrowsingController(IMediaLibrary index) {
            Index = index;
        }

        public IMediaLibrary Index { get; }

        [HttpGet]
        [Route("getMusicFolders")]
        [Route("getMusicFolders.view")]
        public ActionResult<Response> GetMusicFolders() {

            return new Response {
                Item = new MusicFolders {
                    musicFolder = Index.GetRootFolders()
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
            MediaLibraryEntry dir = Index.GetFolder(id);

            MediaLibraryEntry rootDir = Index.GetRootFolderFor(id);

            return new Response {
                Item = new Directory {
                    id = dir.Id.ToString(CultureInfo.InvariantCulture),
                    name = dir.Name,
                    parent = dir.ParentId.ToString(CultureInfo.InvariantCulture),
                    child = Index.GetChildEntries(id)
                        .Select(i => new Child {
                            id = i.Id.ToString(CultureInfo.InvariantCulture),
                            parent = i.ParentId.ToString(CultureInfo.InvariantCulture),
                            artist = i.Artist,
                            title = i.Name,
                            album = dir.Name,
                            isDir = i.IsFolder,
                            track = i.TrackNumber ?? 0,
                            trackSpecified = i.TrackNumber.HasValue,
                            coverArt = (i.IsFolder ? i.Id : i.ParentId).ToString(CultureInfo.InvariantCulture),
                            duration = i.Duration.HasValue ? (int)System.Math.Ceiling(i.Duration.Value.TotalSeconds) : 0,
                            durationSpecified = i.Duration.HasValue,
                            path = System.IO.Path.GetRelativePath(rootDir.Path, i.Path),
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
            IEnumerable<MediaLibraryEntry> entries = Index.GetRootFolders()
                .SelectMany(f => Index.GetChildFolders(f.Id));

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
