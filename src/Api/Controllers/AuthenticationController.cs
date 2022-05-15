using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Auricular.DataTransfer;
using Auricular.Api.Settings;
using Auricular.DataAccess;
using Auricular.DataTransfer.Security;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Auricular.Api.Controllers {
    [Route("auth")]
    [ApiController]
    [FormatFilter]
    public class AuthenticationController : ControllerBase {
        public AuthenticationController(IOptions<ApplicationSettings> appSettings) {
            AppSettings = appSettings;
        }

        public IOptions<ApplicationSettings> AppSettings { get; }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public ActionResult<LoginResponse> Login(LoginRequest loginRequest) {
            if (loginRequest == null) {
                return BadRequest();
            }

            if(loginRequest.UserName == AppSettings.Value.UserName && loginRequest.Password == AppSettings.Value.Password) {
                var principal = new ClaimsPrincipal(new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, AppSettings.Value.UserName),
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme));

                HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties {
                        IsPersistent = true
                    });

                return Ok(new LoginResponse {
                    UserName = AppSettings.Value.UserName,
                });
            }

            return Unauthorized();
        }
    }
}
