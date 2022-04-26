using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Auricular.Api.Settings;
using Auricular.DataAccess;
using Auricular.DataAccess.Entities;

namespace Auricular.Api.Media {
    public class MediaScanner {
        private readonly object scanLockObject = new object();

        public MediaScanner(IOptions<MediaLibrarySettings> settings, IMediaLibrary library, ILogger<MediaScanner> logger) {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Library = library ?? throw new ArgumentNullException(nameof(library));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsScanInProgress { get; private set; }
        public IOptions<MediaLibrarySettings> Settings { get; }
        public IMediaLibrary Library { get; }
        public ILogger<MediaScanner> Logger { get; }

        public void Scan() {
            lock (scanLockObject) {
                if (IsScanInProgress) {
                    return;
                }

                IsScanInProgress = true;
            }

            Logger.LogInformation("Starting scan...");

            DirectoryInfo directoryInfo = new DirectoryInfo(Settings.Value.SourceDirectory);
            int numFilesAdded = 0;
            Stopwatch sw = Stopwatch.StartNew();

            Scan(-1, ref numFilesAdded, directoryInfo);

            int numFilesRemoved = PurgeMissingEntries(directoryInfo);

            Logger.LogInformation("Scan complete. Added {0} and removed {1} files in {2}ms.",
                numFilesAdded, numFilesRemoved, sw.ElapsedMilliseconds);

            lock (scanLockObject) {
                IsScanInProgress = false;
            }
        }

        private int PurgeMissingEntries(DirectoryInfo directoryInfo) {
            int count = 0;
            IEnumerable<MediaLibraryEntry> entries = Library.GetAllEntries().ToList();

            foreach (MediaLibraryEntry entry in entries) {
                if ((entry.IsFolder && !Directory.Exists(entry.Path)) ||
                    (!entry.IsFolder && !File.Exists(entry.Path))) {

                    Library.RemoveEntry(entry);

                    if (!entry.IsFolder) {
                        count++;
                    }
                }
            }

            return count;
        }

        private void Scan(int parentId, ref int numFilesAdded, DirectoryInfo info) {
            MediaLibraryEntry directoryEntry = GetOrCreateDirectoryEntry(parentId, info);

            IEnumerable<FileSystemInfo> fileSystemEntries =
                info.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

            HashSet<string> albumNames = new HashSet<string>();

            foreach (FileSystemInfo info2 in fileSystemEntries) {
                if (info2 is DirectoryInfo directoryInfo) {
                    if (info2.Name != "__MACOSX") {
                        Scan(directoryEntry.Id, ref numFilesAdded, directoryInfo);
                    }
                } else if (info2.Name.EndsWith(".mp3")) {
                    bool added = AddOrUpdateFileEntry(directoryEntry.Id, (FileInfo)info2, albumNames);
                    if (added) {
                        numFilesAdded++;
                    }
                }
            }

            if (albumNames.Count == 1) {
                directoryEntry.Name = albumNames.Single();
                Library.UpdateEntry(directoryEntry);
            }
        }

        private MediaLibraryEntry GetOrCreateDirectoryEntry(int parentId, DirectoryInfo info) {
            MediaLibraryEntry entry = Library.GetEntryByPath(info.FullName);

            if (entry == null) {
                entry = new MediaLibraryEntry {
                    ParentId = parentId,
                    Path = info.FullName,
                    Name = info.Name,
                    IsFolder = true,
                    AddedUtc = DateTime.UtcNow,
                };

                Logger.LogInformation("Scan: Adding directory: {0}", info.FullName);

                Library.AddEntry(entry);
            }

            return entry;
        }

        private bool AddOrUpdateFileEntry(int parentId, FileInfo info, HashSet<string> albumNames) {
            string artist = null;
            string title = null;
            int? trackNumber = null;
            TimeSpan? duration = null;

            using (var file = TagLib.File.Create(info.FullName)) {
                if (file.Tag != null) {
                    title = file.Tag.Title ??
                        Path.GetFileNameWithoutExtension(info.Name);

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

            bool added;
            MediaLibraryEntry existingEntry = Library.GetEntryByPath(info.FullName);
            MediaLibraryEntry newEntry;

            if (existingEntry == null) {
                Logger.LogInformation ("Scan: Adding file: {0}", info.FullName);

                added = true;
                newEntry = new MediaLibraryEntry {
                    IsFolder = false,
                    ParentId = parentId,
                    Path = info.FullName,
                    AddedUtc = DateTime.UtcNow,
                };
            } else {
                newEntry = existingEntry.Copy();
                added = false;
            }

            newEntry.Name = title;
            newEntry.TrackNumber = trackNumber;
            newEntry.Artist = artist;
            newEntry.Duration = duration;

            if(existingEntry != null) {
                if (existingEntry.Equals(newEntry)) {
                    return false;
                }

                Library.RemoveEntry(existingEntry);
            }

            Library.AddEntry(newEntry);

            return added;
        }
    }
}
