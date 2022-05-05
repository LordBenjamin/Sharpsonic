using Auricular.DataTransfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    [Authorize]
    public class BookmarksController : ControllerBase {

        public BookmarksController() {
        }

        [HttpGet]
        [Route("createBookmark")]
        [Route("createBookmark.view")]
        public ActionResult<Response> CreateBookmark(int id, int position, string comment) {
            return new Response();
        }
    }
}
