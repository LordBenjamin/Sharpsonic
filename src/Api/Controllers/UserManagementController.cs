using Sharpsonic.Api.DataTransfer;
using Sharpsonic.Api.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using Sharpsonic.Api.Settings;

namespace Sharpsonic.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    public class UserManagementController : ControllerBase {
        public UserManagementController(IOptions<ApplicationSettings> appSettings, MediaIndex index) {
            Settings = appSettings.Value;
            Index = index;
        }

        public ApplicationSettings Settings { get; }
        public MediaIndex Index { get; }

        [HttpGet]
        [Route("getUser")]
        [Route("getUser.view")]
        public ActionResult<Response> GetUser(string userName) {
            if (!string.Equals(Settings.UserName, userName, System.StringComparison.OrdinalIgnoreCase)) {
                return new Response {
                    Item = new Error {
                        code = 50, // Not authorized
                        message = "User is not authorized for the given operation.",
                    }
                };
            }

            // TODO: Enable these as features are added. Eventually implement actual permissions.
            return new Response {
                ItemElementName = ItemChoiceType.user,
                Item = new User {
                    username = userName,
                    adminRole = false,
                    commentRole = false,
                    downloadRole = false,
                    coverArtRole = true,
                    jukeboxRole = false,
                    playlistRole =false,
                    podcastRole = false,
                    scrobblingEnabled = false,
                    settingsRole = false,
                    shareRole = false,
                    streamRole = true,
                    uploadRole = false,
                    videoConversionRole = false,

                    folder = Index.Entries
                        .Where(i => i.IsFolder && i.ParentId == -1)
                        .Select(i => i.Id)
                        .ToArray(),
                }
            };
        }
    }
}
