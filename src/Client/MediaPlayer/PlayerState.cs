using System;
using System.Threading.Tasks;
using Howler.Blazor.Components;

namespace Auricular.Client.MediaPlayer {
    public class PlayerState {
        private readonly IHowl howl;
        private int? currentSoundId;

        public event EventHandler<EventArgs>? PlayerStateChanged;

        public PlayerState(IHowl howl) {
            this.howl = howl;
        }

        public Uri? StreamUri { get; private set; }
        public string NowPlaying { get; private set; } = "Player";

        public async ValueTask Play(Uri streamUri, string nowPlaying) {
            StreamUri = streamUri;

            NowPlaying = nowPlaying;

            await Stop();

            currentSoundId = await howl.Play(new HowlOptions {
                Sources = new[] { streamUri.AbsoluteUri },
                Formats = new[] { "mp3" },
                Html5 = true,
                Loop = false,
            });

            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public async ValueTask Stop() {
            if (currentSoundId is int soundId) {
                await howl.Stop(soundId);
                currentSoundId = null;
            }

            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsPlaying => currentSoundId.HasValue;
    }
}
