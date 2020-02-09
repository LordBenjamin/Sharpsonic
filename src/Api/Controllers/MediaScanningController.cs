using Api.DataTransfer;
using Api.Media;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers {
    [Route("rest")]
    [ApiController]
    [Produces("application/xml")]
    public class MediaScanningController {
        public MediaScanningController(MediaIndex index) {
            Index = index;
        }

        public MediaIndex Index { get; }

        [HttpGet]
        [Route("getScanStatus")]
        [Route("getScanStatus.view")]
        public ActionResult<Response> GetScanStatus() {
            return new Response {
                Item = new ScanStatus() {
                    scanning = Index.IsScanInProgress,
                },
                ItemElementName = ItemChoiceType.scanStatus,
            };
        }

        [HttpGet]
        [Route("startScan")]
        [Route("startScan.view")]
        public ActionResult<Response> StartScan() {
            // TODO: Should this be a "hosted service"?
            Task.Run(() => Index.Scan())
                .ConfigureAwait(false);

            return new Response {
                Item = new ScanStatus() {
                    scanning = true,
                },
                ItemElementName = ItemChoiceType.scanStatus,
            };
        }
    }
}
