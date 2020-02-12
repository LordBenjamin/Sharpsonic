using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sharpsonic.Api.Media.InMemory;
using Sharpsonic.Api.Settings;

namespace Sharpsonic.Api.Media {

    public class MediaLibraryService : IHostedService {

        public MediaLibraryService(IOptions<MediaLibrarySettings> settings, InMemoryMediaLibrary library, MediaScanner scanner) {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Library = library ?? throw new ArgumentNullException(nameof(library));
            Scanner = scanner ?? throw new ArgumentNullException(nameof(library));
        }

        public IOptions<MediaLibrarySettings> Settings { get; }
        public InMemoryMediaLibrary Library { get; }
        public MediaScanner Scanner { get; }

        public async Task StartAsync(CancellationToken cancellationToken) {
            await Task.Run(() => Scanner.Scan())
                .ConfigureAwait(false);

            await Task.CompletedTask
                .ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }
}
