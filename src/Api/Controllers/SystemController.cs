using Api.DataTransfer;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers {
    [Route("rest")]
    [ApiController]
    [Produces("application/xml")]
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
