using Sharpsonic.Api.DataTransfer;
using Microsoft.AspNetCore.Mvc;

namespace Sharpsonic.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
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
