using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Auricular.Client.Services;
using Howler.Blazor.Components;
using Howler.Blazor.Components.Events;
using Microsoft.JSInterop;

namespace Auricular.Client.MediaPlayer {
    public class PlayerState {
        private readonly IHowl howl;
        private int? currentSoundId;

        public event EventHandler<EventArgs>? PlayerStateChanged;
        public event EventHandler<EventArgs>? Step;

        public PlayerState(IHowl howl, MediaRetrievalService mediaRetrievalService) {
            this.howl = howl;
            howl.OnEnd += Howl_OnEnd;
            howl.OnLoadError += Howl_OnError;
            howl.OnPlayError += Howl_OnError;
            howl.OnPlay += Howl_OnPlay;
            howl.OnSeek += Howl_OnSeek;
            timer = new Timer(500);
            timer.Elapsed += Timer_Elapsed;
            MediaRetrievalService = mediaRetrievalService;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e) {
            if (IsPlaying) {
                OnStep();
            }
        }

        private void Howl_OnError(HowlErrorEventArgs e) {
            Console.WriteLine(e.Error);
        }

        public bool ShouldStopAtEndOfChapter { get; set; }
        public Uri? StreamUri { get; private set; }
        public PlaylistItem? CurrentItem { get; private set; }

        public Playlist Playlist { get; private set; } = Playlist.Empty;

        public void SetPlaylist(Playlist playlist) {
            Playlist = playlist;
        }

        public async ValueTask Play(Playlist playlist) {
            ArgumentNullException.ThrowIfNull(playlist);

            SetPlaylist(playlist);
            await Play(playlist.Items[0]);
        }

        public async ValueTask Play() {
            if (currentSoundId is int soundId) {
                await howl.Play(soundId);

                timer.Start();
                PlayerStateChanged?.Invoke(this, EventArgs.Empty);
            } else if (CurrentItem is PlaylistItem playlistItem) {
                await Play(playlistItem);
            }
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

            timer.Start();
            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Howl_OnPlay(HowlPlayEventArgs? e) {
            if (e?.SoundId is null || e.SoundId != currentSoundId) {
                return;
            }

            TotalTime = e.TotalTime;

            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public async ValueTask Pause() {
            if (currentSoundId is int soundId) {
                await howl.Pause(soundId);
            }

            timer.Stop();
            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public async ValueTask Stop() {
            if (currentSoundId is int soundId) {
                currentSoundId = null;
                TotalTime = null;

                await howl.Stop(soundId);
            }

            timer.Stop();
            PlayerStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void Howl_OnEnd(HowlEventArgs e) {
            if (e.SoundId == currentSoundId) {
                // TODO: End vs stopped?

                // Search list to ensure we have correct object ref, not just value equality
                PlaylistItem? current = Playlist.Items
                    .Where(i => i.Equals(CurrentItem))
                    .SingleOrDefault();

                if (current == null) {
                    return;
                }

                int index = Array.IndexOf(Playlist.Items, current);
                if (index == Playlist.Length) {
                    return;
                }

                await Play(Playlist.Items[index + 1]);

                if (ShouldStopAtEndOfChapter) {
                    await Stop();
                    return;
                }
            }
        }

        public bool IsPlayingItem(PlaylistItem item) {
            ArgumentNullException.ThrowIfNull(item);
            return item.Equals(CurrentItem);
        }

        public async ValueTask<TimeSpan?> GetCurrentTime() {
            return currentSoundId is int id ? await howl.GetCurrentTime(id) : null;
        }

        public async ValueTask Seek(TimeSpan position) {
            if (currentSoundId is int id) {
                timer.Stop();

                await howl.Seek(id, position);

                await Task.Delay(100);
                OnStep();
                timer.Start();
            }
        }

        // TODO: This is not hooked up in Blazor Howler
        private void Howl_OnSeek(HowlEventArgs e) {
            if (e?.SoundId is null || e.SoundId != currentSoundId) {
                return;
            }

            OnStep();
            timer.Start();
        }

        private void OnStep() {
            Step?.Invoke(this, EventArgs.Empty);
        }

        public bool IsPlaying => currentSoundId.HasValue && timer.Enabled;

        public TimeSpan? TotalTime { get; private set; }

        private Timer timer;

        public MediaRetrievalService MediaRetrievalService { get; }
    }
}
