using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Sharpsonic.Api.Media.InMemory;
using Sharpsonic.Api.Settings;

namespace Sharpsonic.Api.Media {
    public class MediaScanner {
        private readonly object scanLockObject = new object();

        public MediaScanner(IOptions<MediaLibrarySettings> settings, InMemoryMediaLibrary library) {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Library = library ?? throw new ArgumentNullException(nameof(library));
        }

        public bool IsScanInProgress { get; private set; }
        public IOptions<MediaLibrarySettings> Settings { get; }
        public InMemoryMediaLibrary Library { get; }

        public void Scan() {
            lock (scanLockObject) {
                if (IsScanInProgress) {
                    return;
                }

                IsScanInProgress = true;
            }

            Scan(-1, new DirectoryInfo(Settings.Value.SourceDirectory));

            lock (scanLockObject) {
                IsScanInProgress = false;
            }
        }

        private void Scan(int parentId, DirectoryInfo info) {
            MediaLibraryEntry directoryEntry = AddDirectoryEntry(parentId, info);

            IEnumerable<FileSystemInfo> fileSystemEntries =
                info.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

            HashSet<string> albumNames = new HashSet<string>();

            foreach (FileSystemInfo info2 in fileSystemEntries) {
                if (info2 is DirectoryInfo directoryInfo) {
                    Scan(directoryEntry.Id, directoryInfo);
                } else if (info2.Name.EndsWith(".mp3")) {
                    string artist = null;
                    string title = null;
                    int? trackNumber = null;
                    TimeSpan? duration = null;

                    using (var file = TagLib.File.Create(info2.FullName)) {
                        if (file.Tag != null) {
                            title = file.Tag.Title ??
                                Path.GetFileNameWithoutExtension(info2.Name);

                            trackNumber = file.Tag.Track > 0 ?
                                (int)file.Tag.Track : (int?)null;

                            string album = file.Tag.Album?.Trim();

                            if (!string.IsNullOrWhiteSpace(album)) {
                                albumNames.Add(album);
                            }

                            artist = !string.IsNullOrWhiteSpace(file.Tag.JoinedAlbumArtists) ? file.Tag.JoinedAlbumArtists?.Trim() :
                                !string.IsNullOrWhiteSpace(file.Tag.FirstAlbumArtist) ? file.Tag.FirstAlbumArtist?.Trim() :
                                !string.IsNullOrWhiteSpace(file.Tag.FirstPerformer) ? file.Tag.FirstPerformer?.Trim() :
#pragma warning disable CS0618 // Type or member is obsolete
                                 file.Tag.Artists?.Length > 0 ? string.Join('/', file.Tag.Artists) : null;
#pragma warning restore CS0618 // Type or member is obsolete

                            duration = file.Properties.Duration;
                        }
                    }

                    AddEntry(directoryEntry.Id,
                        name: title,
                        path: info2.FullName,
                        trackNumber: trackNumber,
                        artist: artist,
                        duration: duration);
                }
            }

            if (albumNames.Count == 1) {
                directoryEntry.Name = albumNames.Single();
            }
        }

        private MediaLibraryEntry AddDirectoryEntry(int parentId, FileSystemInfo info) {
            return AddEntry(parentId, info.Name, info.FullName, isFolder: true);
        }

        private MediaLibraryEntry AddEntry(
            int parentId, string name, string path, bool isFolder = false,
            int? trackNumber = null, string artist = null, TimeSpan? duration = null) {

            var entry = new MediaLibraryEntry {
                ParentId = parentId,
                IsFolder = isFolder,
                Name = name,
                Path = path,
                TrackNumber = trackNumber,
                Artist = artist,
                Duration = duration,
            };

            Library.AddEntry(entry);

            return entry;
        }
    }
}
