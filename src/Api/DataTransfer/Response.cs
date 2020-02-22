using Auricular.Api.Middleware;

namespace Auricular.Api.DataTransfer {
    public partial class Response {
        public Response() {
            version = SubsonicApiVersioningMiddleware.ServerVersion.ToString(3);
        }
    }
}
