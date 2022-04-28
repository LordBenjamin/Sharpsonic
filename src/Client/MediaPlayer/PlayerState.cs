using System;
using System.Linq;
using System.Threading.Tasks;
using Auricular.Client.Services;
using Howler.Blazor.Components;
using Howler.Blazor.Components.Events;

namespace Auricular.Client.MediaPlayer {
    public class PlayerState {
        private readonly IHowl howl;
        private int? currentSoundId;

        public event EventHandler<EventArgs>? PlayerStateChanged;

        public PlayerState(IHowl howl, MediaRetrievalService mediaRetrievalService) {
            this.howl = howl;
            howl.OnEnd += Howl_OnEnd;
            MediaRetrievalService = mediaRetrievalService;
        }

        public Uri? StreamUri { get; private set; }
        public PlaylistItem CurrentItem { get; private set; }

        public Playlist Playlist { get; private set; } = Playlist.Empty;

        public void SetPlaylist(Playlist playlist) {
            Playlist = playlist;
        }

        public async ValueTask Play(Playlist playlist) {
            ArgumentNullException.ThrowIfNull(playlist);

            SetPlaylist(playlist);
            await Play(playlist.Items[0]);
        }

        public async ValueTask Play(PlaylistItem item) {
            ArgumentNullException.ThrowIfNull(item);

            if (!Playlist.Items.Contains(item)) {
                throw new InvalidOperationException();
            }

            Uri uri = MediaRetrievalService.GetItemUri(item.Id);

            CurrentItem = item;
            PlayerStateChanged?.Invoke(this, EventArgs.Empty);

            await PlaySound(uri);
        }

        private async ValueTask PlaySound(Uri streamUri) {
            StreamUri = streamUri;

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
                currentSoundId = null;
                await howl.Stop(soundId);
            }

            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void Howl_OnEnd(HowlEventArgs e) {
            if (e.SoundId == currentSoundId) {
                // TODO: End vs stopped?

                // Search list to ensure we have correct object ref, not just value equality
                PlaylistItem? current = Playlist.Items
                    .Where(i => i.Equals(CurrentItem))
                    .SingleOrDefault();

                if(current == null) {
                    return;
                }

                int index = Array.IndexOf(Playlist.Items, current);
                if(index == Playlist.Length) {
                    return;
                }

                await Play(Playlist.Items[index + 1]);
            }
        }

        public bool IsPlayingItem(PlaylistItem item) {
            ArgumentNullException.ThrowIfNull(item);
            return item.Equals(CurrentItem);
        }

        public bool IsPlaying => currentSoundId.HasValue;

        public MediaRetrievalService MediaRetrievalService { get; }
    }
}
