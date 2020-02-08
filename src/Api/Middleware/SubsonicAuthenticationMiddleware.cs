using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.Middleware
{
    public class SubsonicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApplicationSettings options;

        public SubsonicAuthenticationMiddleware(
            RequestDelegate next, IOptions<ApplicationSettings> options)
        {
            _next = next;
            this.options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            string userName = GetQueryParameter(context, "u");
            string token = GetQueryParameter(context, "t");
            string salt = GetQueryParameter(context, "s");

            bool isAuthenticated =
                AreEquivalent(context, options.UserName, userName) &&
                AreEquivalent(context, Hash(options.Password, salt), token);

            if (isAuthenticated)
            {
                Claim[] claims = new[] {
                    new Claim("name", userName),
                    new Claim(ClaimTypes.Role, "User"),
                };

                ClaimsIdentity identity = new ClaimsIdentity(claims, "Basic");
                context.User = new ClaimsPrincipal(identity);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        private static string GetQueryParameter(HttpContext context, string parameterName)
        {
            return context.Request.Query[parameterName];
        }

        private bool AreEquivalent(HttpContext context, string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        private static string Hash(string password, string salt)
        {
            string toHash = password + salt;

            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(toHash);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
