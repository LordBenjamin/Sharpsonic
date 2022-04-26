namespace Auricular.DataTransfer {
    public partial class Response {
        public Response() {
            version = SubsonicCompatibility.ServerVersion.ToString(3);
        }
    }
}
