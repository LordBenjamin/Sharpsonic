using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Auricular.DataTransfer;

namespace Auricular.Client.Services {
    public class BrowsingService {
        public BrowsingService(HttpClient httpClient) {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task<DirectoryListingResponse> GetDirectory(int id) {
            DirectoryListingResponse? result = await HttpClient.GetFromJsonAsync<DirectoryListingResponse>(
                $"rest/getMusicDirectory?id={id.ToString(CultureInfo.InvariantCulture)}");

            return result!;
        }
    }
}
