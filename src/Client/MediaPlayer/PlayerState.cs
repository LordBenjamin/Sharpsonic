using System;

namespace Auricular.Client.MediaPlayer {
    public class PlayerState {
        public event EventHandler<EventArgs> PlayerStateChanged;

        public string NowPlaying { get; private set; } = "Player";

        public void Play(string nowPlaying) {
            NowPlaying = nowPlaying;

            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
