using Api.Middleware;

namespace Api.DataTransfer {
    public partial class Response {
        public Response() {
            version = SubsonicApiVersioningMiddleware.ServerVersion.ToString(3);
        }
    }
}
