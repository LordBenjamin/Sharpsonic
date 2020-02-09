using Sharpsonic.Api.DataTransfer;
using Microsoft.AspNetCore.Mvc;
using Sharpsonic.Api.Media;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace Sharpsonic.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    public class AlbumSongListController : ControllerBase {

        public AlbumSongListController(MediaIndex index) {
            Index = index;
        }

        public MediaIndex Index { get; }

        [HttpGet]
        [Route("getAlbumList")]
        [Route("getAlbumList.view")]
        public ActionResult<Response> GetAlbumList(
            string type,
            int size = 10,
            int offset = 0,
            int fromYear = -1,
            int toYear = int.MaxValue,
            string genre = null,
            int? musicFolderId = null) {

            IEnumerable<MediaIndexEntry> query = Index.Entries
                .Where(i => i.ParentId >= 0)
                .Where(i => i.IsFolder);

            if (musicFolderId.HasValue) {
                query = query.Where(i => i.ParentId == musicFolderId.Value);
            }

            // TODO: continue filtering query according to parameters

            if (type == "random") {
                Random random = new Random();
                query = query.OrderBy(i => random.NextDouble());
            } else if (type == "alphabeticalByName ") {
                query = query.OrderBy(i => i.Name);
            } else if (type == "alphabeticalByArtist ") {
                query = query.OrderBy(i => i.Artist)
                    .ThenBy(i => i.Name);
            }

            query = query
                .Skip(offset)
                .Take(size);

            return new Response {
                ItemElementName = ItemChoiceType.albumList,
                Item = new AlbumList {
                    album = query
                        .Select(i => new Child {
                            id = i.Id.ToString(CultureInfo.InvariantCulture),
                            parent = i.ParentId.ToString(CultureInfo.InvariantCulture),
                            title = i.Name,
                            artist = i.Artist,
                            isDir = true,
                            coverArt = i.Id.ToString(CultureInfo.InvariantCulture),
                        })
                        .ToArray(),
                }
            };
        }
    }
}
