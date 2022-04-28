using System;

namespace Auricular.Client.MediaPlayer {
    public class Playlist {
        public static Playlist Empty { get; } = new Playlist(Array.Empty<PlaylistItem>());

        public Playlist(PlaylistItem[] items) {
            ArgumentNullException.ThrowIfNull(items);
            Items = items;
        }

        public PlaylistItem[] Items { get; }

        public int Length => Items?.Length ?? 0;
    }
}
