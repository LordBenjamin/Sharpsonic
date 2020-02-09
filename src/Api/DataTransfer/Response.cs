using Sharpsonic.Api.Middleware;

namespace Sharpsonic.Api.DataTransfer {
    public partial class Response {
        public Response() {
            version = SubsonicApiVersioningMiddleware.ServerVersion.ToString(3);
        }
    }
}
