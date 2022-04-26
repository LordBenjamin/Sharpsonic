using System;
using System.Threading.Tasks;
using Howler.Blazor.Components;

namespace Auricular.Client.MediaPlayer {
    public class PlayerState {
        private readonly IHowl howl;
        private int? currentSoundId;

        public event EventHandler<EventArgs> PlayerStateChanged;

        public PlayerState(IHowl howl) {
            this.howl = howl;
        }

        public string NowPlaying { get; private set; } = "Player";

        public async Task Play(string url, string nowPlaying) {
            NowPlaying = nowPlaying;

            PlayerStateChanged?.Invoke(this, EventArgs.Empty);

            if (currentSoundId is int soundId) {
                await howl.Stop(soundId);
                currentSoundId = null;
            }

            currentSoundId = await howl.Play(new HowlOptions {
                Sources = new[] { url },
                Formats = new[] { "mp3" },
                Html5 = true,
                Loop = false,
            });
        }
    }
}
