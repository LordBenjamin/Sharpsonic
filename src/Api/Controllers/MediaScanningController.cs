using System;
using System.Threading.Tasks;
using Auricular.Api.Media;
using Auricular.DataAccess;
using Auricular.DataTransfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    [Authorize]
    public class MediaScanningController {
        public MediaScanningController(IMediaLibrary index, MediaScanner scanner) {
            Index = index ?? throw new ArgumentNullException(nameof(index));
            Scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        }

        public IMediaLibrary Index { get; }
        public MediaScanner Scanner { get; }

        [HttpGet]
        [Route("getScanStatus")]
        [Route("getScanStatus.view")]
        public ActionResult<Response> GetScanStatus() {
            return new Response {
                Item = new ScanStatus() {
                    scanning = Scanner.IsScanInProgress,
                    count = Index.GetFileCount(),
                    countSpecified = true,
                },
                ItemElementName = ItemChoiceType.scanStatus,
            };
        }

        [HttpGet]
        [Route("startScan")]
        [Route("startScan.view")]
        public ActionResult<Response> StartScan() {
            // TODO: Should this use the hosted service?
            Task.Run(() => Scanner.Scan())
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
