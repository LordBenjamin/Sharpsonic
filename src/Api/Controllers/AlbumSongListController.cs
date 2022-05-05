using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Auricular.DataTransfer;
using Auricular.DataAccess;
using Auricular.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    [Authorize]
    public class AlbumSongListController : ControllerBase {

        public AlbumSongListController(IMediaLibrary index) {
            Index = index;
        }

        public IMediaLibrary Index { get; }

        [HttpGet]
        [Route("getAlbumList")]
        [Route("getAlbumList.view")]
        public ActionResult<AlbumListResponse> GetAlbumList(
            string type = "alphabeticalByName",
            int size = 0,
            int offset = 0,
            int fromYear = -1,
            int toYear = int.MaxValue,
            string genre = null,
            int? musicFolderId = null) {

            IEnumerable<MediaLibraryEntry> query = Index.GetNonRootFolders();

            if (musicFolderId.HasValue) {
                query = query.Where(i => i.ParentId == musicFolderId.Value);
            }

            // TODO: continue filtering query according to parameters

            if (type == "random") {
                Random random = new Random();
                query = query.OrderBy(i => random.NextDouble());
            } else if (type == "alphabeticalByName") {
                query = query.OrderBy(i => i.Name);
            } else if (type == "alphabeticalByArtist") {
                query = query.OrderBy(i => i.Artist)
                    .ThenBy(i => i.Name);
            } else if (type == "newest") {
                query = query.OrderByDescending(i => i.AddedUtc)
                    .ThenBy(i => i.Name);
            } else if (type == "recent") {
                query = query.Where(i => i.LastPlayedUtc.HasValue)
                    .OrderByDescending(i => i.LastPlayedUtc)
                    .ThenBy(i => i.Name);
            }

            query = query
                .Skip(offset);

            if (size > 0) {
                query = query
                    .Take(size);
            }

            return new AlbumListResponse {
                Albums = query
                .Select(i => new Album {
                    Id = i.Id,
                    ParentId = i.ParentId,
                    Title = i.Name,
                    Artist = i.Artist,
                    IsDir = true,
                    CoverArtId = i.Id,
                })
                .ToArray(),
            };
        }
    }
}
