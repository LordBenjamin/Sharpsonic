using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Auricular.DataTransfer;

namespace Auricular.Client.Services {
    public class AlbumSongListService {
        public AlbumSongListService(HttpClient httpClient) {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task<Album[]?> GetAlbums() {
            return (await HttpClient.GetFromJsonAsync<AlbumListResponse>("rest/getAlbumList"))?.Albums;
        }
    }
}
