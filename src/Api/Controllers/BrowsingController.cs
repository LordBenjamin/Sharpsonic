using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Auricular.DataTransfer;
using Auricular.DataAccess;
using Auricular.DataAccess.Entities;
using TimeSpan = System.TimeSpan;

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
        public ActionResult<DirectoryListingResponse> GetMusicDirectory(int id) {
            MediaLibraryEntry dir = Index.GetFolder(id);

            MediaLibraryEntry rootDir = Index.GetRootFolderFor(id);

            return new DirectoryListingResponse {
                    Id = dir.Id,
                    Name = dir.Name,
                    ParentId = dir.ParentId,
                    Items = Index.GetChildEntries(id)
                        .Select(i => new DirectoryListingItem {
                            Id = i.Id,
                            ParentId = i.ParentId,
                            Artist = i.Artist,
                            Title = i.Name,
                            Album = dir.Name,
                            IsDir = i.IsFolder,
                            Track = i.TrackNumber ?? 0,
                            TrackSpecified = i.TrackNumber.HasValue,
                            CoverArt = (i.IsFolder ? i.Id : i.ParentId),
                            Duration = i.Duration ?? TimeSpan.Zero,
                            DurationSpecified = i.Duration.HasValue,
                            Path = System.IO.Path.GetRelativePath(rootDir.Path, i.Path),
                        })
                        .OrderBy(i => i.TrackSpecified)
                        .ThenBy(i => i.Album)
                        .ThenBy(i => i.Track)
                        .ThenBy(i => i.Title)
                        .ToArray(),
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
