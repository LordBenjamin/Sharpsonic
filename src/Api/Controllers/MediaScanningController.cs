using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sharpsonic.Api.DataTransfer;
using Sharpsonic.Api.Media;
using Sharpsonic.DataAccess;

namespace Sharpsonic.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
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
