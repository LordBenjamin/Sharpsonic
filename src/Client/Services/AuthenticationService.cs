using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Auricular.DataTransfer.Security;

namespace Auricular.Client.Services {
    public class AuthenticationService {
        public AuthenticationService(HttpClient httpClient) {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task<LoginResponse?> TryLogin(string userName, string password) {
            using HttpResponseMessage? response = await HttpClient.PostAsJsonAsync(
                $"auth/login", new LoginRequest {
                    UserName = userName,
                    Password = password,
                });

            if(response.StatusCode == HttpStatusCode.Unauthorized) {
                return null; // 401 means the username / password was wrong
            }

            response.EnsureSuccessStatusCode();

            LoginResponse? result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result!;
        }
    }
}
