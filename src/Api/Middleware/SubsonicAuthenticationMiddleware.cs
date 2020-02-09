using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.Middleware {
    public class SubsonicAuthenticationMiddleware {
        private readonly RequestDelegate _next;
        private readonly ApplicationSettings options;

        public SubsonicAuthenticationMiddleware(
            RequestDelegate next, IOptions<ApplicationSettings> options) {
            _next = next;
            this.options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context) {

            string userName = GetQueryParameter(context, "u");
            string token = GetQueryParameter(context, "t");
            string salt = GetQueryParameter(context, "s");
            string password = GetQueryParameter(context, "p");

            if (!AreEquivalent(context, options.UserName, userName)) {
                await WriteAuthFailure(context);
                return;
            }

            bool passwordAccepted;

            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(salt)) {
                passwordAccepted = AreEquivalent(context, Hash(options.Password, salt), token);
            }
            else {
                passwordAccepted = string.Equals(
                    options.Password, DecodePlaintextPassword(password), StringComparison.Ordinal);
            }

            if (!passwordAccepted) {
                await WriteAuthFailure(context);
                return;

            }

            Claim[] claims = new[] {
                new Claim("name", userName),
                new Claim(ClaimTypes.Role, "User"),
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "Basic");
            context.User = new ClaimsPrincipal(identity);

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        // TODO: What if a user's password legitimately stars with "enc:"??
        private string DecodePlaintextPassword(string password) {
            if (!password.StartsWith("enc:", StringComparison.OrdinalIgnoreCase)) {
                return password; // Totally plaintext
            }

            return Encoding.UTF8.GetString(HexStringToBytes(password.Substring(4)));
        }

        private async Task WriteAuthFailure(HttpContext context) {
            Version version = SubsonicApiVersioningMiddleware.ServerVersion;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "text/xml";

            await context.Response.WriteAsync(
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
$"<subsonic-response xmlns=\"http://subsonic.org/restapi\" status=\"failed\" version=\"{version.ToString(3)}\">\n" +
"   <error code=\"40\" message=\"Wrong username or password.\"/>\n" +
"</subsonic-response>");
        }

        private static string GetQueryParameter(HttpContext context, string parameterName) {
            return context.Request.Query[parameterName];
        }

        private bool AreEquivalent(HttpContext context, string str1, string str2) {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        private static string Hash(string password, string salt) {
            string toHash = password + salt;

            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(toHash);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        // https://codereview.stackexchange.com/a/97971
        public static byte[] HexStringToBytes(string hexString) {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++) {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }
    }
}
