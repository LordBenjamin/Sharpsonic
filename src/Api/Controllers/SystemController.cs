using Auricular.DataTransfer;
using Microsoft.AspNetCore.Mvc;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    public class SystemController : ControllerBase {
        [HttpGet]
        [Route("ping")]
        [Route("ping.view")]
        public ActionResult<Response> Ping() {
            return new Response();
        }

        [HttpGet]
        [Route("getLicense")]
        [Route("getLicense.view")]
        public ActionResult<Response> GetLicense() {
            return new Response() {
                Item = new License() {
                    valid = true,
                },
                ItemElementName = ItemChoiceType.license,
            };
        }
    }
}
