using Sharpsonic.Api.DataTransfer;
using Sharpsonic.Api.Media;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sharpsonic.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    public class MediaScanningController {
        public MediaScanningController(MediaLibraryService index) {
            Index = index;
        }

        public MediaLibraryService Index { get; }

        [HttpGet]
        [Route("getScanStatus")]
        [Route("getScanStatus.view")]
        public ActionResult<Response> GetScanStatus() {
            return new Response {
                Item = new ScanStatus() {
                    scanning = Index.IsScanInProgress,
                    count = Index.Entries.Where(i => !i.IsFolder).Count(),
                    countSpecified = true,
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
