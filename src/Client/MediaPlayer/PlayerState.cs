using System;
using System.Threading.Tasks;
using Howler.Blazor.Components;

namespace Auricular.Client.MediaPlayer {
    public class PlayerState {
        private readonly IHowl howl;

        public event EventHandler<EventArgs> PlayerStateChanged;

        public PlayerState(IHowl howl) {
            this.howl = howl;
        }

        public string NowPlaying { get; private set; } = "Player";

        public async Task Play(string url, string nowPlaying) {
            NowPlaying = nowPlaying;

            PlayerStateChanged?.Invoke(this, EventArgs.Empty);

            await howl.Play(new HowlOptions {
                Sources = new[] { url },
                Formats = new[] { "mp3" },
                Html5 = true,
                Loop = false,
            });
        }
    }
}
