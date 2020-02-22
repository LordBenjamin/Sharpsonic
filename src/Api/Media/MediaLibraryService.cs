using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Auricular.Api.Settings;
using Auricular.DataAccess;

namespace Auricular.Api.Media {

    public class MediaLibraryService : IHostedService {

        public MediaLibraryService(IOptions<MediaLibrarySettings> settings, IMediaLibrary library, MediaScanner scanner) {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Library = library ?? throw new ArgumentNullException(nameof(library));
            Scanner = scanner ?? throw new ArgumentNullException(nameof(library));
        }

        public IOptions<MediaLibrarySettings> Settings { get; }
        public IMediaLibrary Library { get; }
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
