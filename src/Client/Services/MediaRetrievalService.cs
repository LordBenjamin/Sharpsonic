using System;
using System.Globalization;
using System.Net.Http;

namespace Auricular.Client.Services {
    public class MediaRetrievalService {
        public MediaRetrievalService(HttpClient httpClient) {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public Uri GetItemUri(int itemId) {
            return new Uri(HttpClient.BaseAddress!, $"rest/stream?id={itemId.ToString(CultureInfo.InvariantCulture)}");
        }

        public Uri GetCoverArtUri(int itemId) {
            return new Uri(HttpClient.BaseAddress!, $"rest/getCoverArt?id={itemId.ToString(CultureInfo.InvariantCulture)}");
        }
    }
}
