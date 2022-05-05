using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Auricular.DataTransfer;
using Auricular.Api.Settings;
using Auricular.DataAccess;
using Microsoft.AspNetCore.Authorization;

namespace Auricular.Api.Controllers {
    [Route("rest")]
    [ApiController]
    [FormatFilter]
    [Authorize]
    public class UserManagementController : ControllerBase {
        public UserManagementController(IOptions<ApplicationSettings> appSettings, IMediaLibrary index) {
            Settings = appSettings.Value;
            Index = index;
        }

        public ApplicationSettings Settings { get; }
        public IMediaLibrary Index { get; }

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
                    playlistRole = false,
                    podcastRole = false,
                    scrobblingEnabled = false,
                    settingsRole = false,
                    shareRole = false,
                    streamRole = true,
                    uploadRole = false,
                    videoConversionRole = false,

                    folder = Index.GetRootFolders()
                        .Select(i => i.Id)
                        .ToArray(),
                }
            };
        }
    }
}
