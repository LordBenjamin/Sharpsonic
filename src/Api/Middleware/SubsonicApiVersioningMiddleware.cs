using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Auricular.Api.Settings;
using System;
using System.Threading.Tasks;
using Auricular.DataTransfer;

namespace Auricular.Api.Middleware {
    public class SubsonicApiVersioningMiddleware {
        private readonly RequestDelegate _next;
        private readonly ApplicationSettings options;

        public SubsonicApiVersioningMiddleware(
            RequestDelegate next, IOptions<ApplicationSettings> options) {
            _next = next;
            this.options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context) {

            string versionParam = GetQueryParameter(context, "v");


            if (!Version.TryParse(versionParam, out Version version) || version < SubsonicCompatibility.MinimumClientVersion) {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/xml";

                await context.Response.WriteAsync(
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
$"<subsonic-response xmlns=\"http://subsonic.org/restapi\" status=\"failed\" version=\"{SubsonicCompatibility.MinimumClientVersion.ToString(3)}\">\n" +
"   <error code=\"20\" message=\"Incompatible Subsonic REST protocol version. Client must upgrade.\"/>\n" +
"</subsonic-response>");
                return;

            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        private static string GetQueryParameter(HttpContext context, string parameterName) {
            return context.Request.Query[parameterName];
        }
    }
}
