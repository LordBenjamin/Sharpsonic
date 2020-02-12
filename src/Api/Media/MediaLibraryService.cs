using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sharpsonic.Api.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sharpsonic.Api.Media {

    public class MediaLibraryService : IHostedService {
        private readonly object scanLockObject = new object();

        private readonly UniqueIndex<int, MediaLibraryEntry> entriesById;
        private readonly NonUniqueIndex<int, MediaLibraryEntry> entriesByParentId;
        private readonly NonUniqueIndex<bool, MediaLibraryEntry> entriesByIsFolder;

        public MediaLibraryService(IOptions<MediaLibrarySettings> settings)
            : this(settings.Value.SourceDirectory) {

            entriesById = new UniqueIndex<int, MediaLibraryEntry>(v => v.Id);
            entriesByParentId = new NonUniqueIndex<int, MediaLibraryEntry>(v => v.ParentId);
            entriesByIsFolder = new NonUniqueIndex<bool, MediaLibraryEntry>(v => v.IsFolder);
        }

        public MediaLibraryService(string folder) {
            Folder = folder;
        }

        public string Folder { get; }

        public List<MediaLibraryEntry> Entries { get; } = new List<MediaLibraryEntry>();

        public bool IsScanInProgress { get; private set; }

        public void Scan() {
            lock (scanLockObject) {
                if (IsScanInProgress) {
                    return;
                }

                IsScanInProgress = true;
            }

            Entries.Clear();
            entriesById.Clear();
            entriesByParentId.Clear();
            entriesByIsFolder.Clear();

            int i = 0;
            Scan(-1, ref i, new DirectoryInfo(Folder));

            lock (scanLockObject) {
                IsScanInProgress = false;
            }
        }

        private void Scan(int parentId, ref int i, DirectoryInfo info) {
            int folderId = i;
            MediaLibraryEntry directoryEntry = AddDirectoryEntry(parentId, i++, info);

            IEnumerable<FileSystemInfo> fileSystemEntries =
                info.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

            HashSet<string> albumNames = new HashSet<string>();

            foreach (FileSystemInfo info2 in fileSystemEntries) {
                if (info2 is DirectoryInfo directoryInfo) {
                    Scan(folderId, ref i, directoryInfo);
                }
                else if (info2.Name.EndsWith(".mp3")) {
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

                    AddEntry(folderId, i++,
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

        private MediaLibraryEntry AddDirectoryEntry(int parentId, int index, FileSystemInfo info) {
            return AddEntry(parentId, index, info.Name, info.FullName, isFolder: true);
        }

        private MediaLibraryEntry AddEntry(
            int parentId, int id, string name, string path, bool isFolder = false,
            int? trackNumber = null, string artist = null, TimeSpan? duration = null) {

            var entry = new MediaLibraryEntry {
                Id = id,
                ParentId = parentId,
                IsFolder = isFolder,
                Name = name,
                Path = path,
                TrackNumber = trackNumber,
                Artist = artist,
                Duration = duration,
            };

            Entries.Add(entry);
            entriesByParentId.Add(entry);
            entriesById.Add(entry);
            entriesByIsFolder.Add(entry);

            return entry;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            await Task.Run(() => Scan())
                .ConfigureAwait(false);

            await Task.CompletedTask
                .ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        internal IEnumerable<MediaLibraryEntry> GetNonRootFolders() {
            return Entries
                .Where(i => i.ParentId >= 0)
                .Where(i => i.IsFolder);
        }

        internal IEnumerable<MediaLibraryEntry> GetRootFolders() {
            return entriesByParentId.Get(-1)
                .Where(i => i.IsFolder);
        }

        internal MediaLibraryEntry GetEntry(int id) {
            return entriesById.Get(id);
        }

        internal MediaLibraryEntry GetFolder(int id) {
            MediaLibraryEntry item = entriesById.Get(id);
            return item.IsFolder ? item : null;
        }

        internal MediaLibraryEntry GetFile(int id) {
            MediaLibraryEntry item = entriesById.Get(id);
            return item.IsFolder ? null : item;
        }

        internal MediaLibraryEntry GetRootFolderFor(int id) {
            return GetRootFolderFor(GetFolder(id));
        }

        internal MediaLibraryEntry GetRootFolderFor(MediaLibraryEntry dir) {
            MediaLibraryEntry rootDir = dir;
            while (rootDir.ParentId >= 0) {
                rootDir = entriesById.Get(rootDir.ParentId);
            }

            return rootDir;
        }

        internal IEnumerable<MediaLibraryEntry> GetChildEntries(int id) {
            return entriesByParentId.Get(id);
        }

        internal IEnumerable<MediaLibraryEntry> GetChildFolders(int id) {
            return GetChildEntries(id)
                .Where(i => i.IsFolder);
        }

        internal IEnumerable<MediaLibraryEntry> GetChildFiles(int id) {
            return GetChildEntries(id)
                .Where(i => !i.IsFolder);
        }

        internal long GetFileCount() {
            return entriesByIsFolder.Get(false)?.Count ?? 0;
        }
    }
}
