using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Api.Media {

    public class MediaIndex {
        private readonly Dictionary<int, MediaIndexEntry> entriesByParentId = new Dictionary<int, MediaIndexEntry>();

        public MediaIndex(string folder) {
            Folder = folder;
        }

        public string Folder { get; }

        public List<MediaIndexEntry> Entries { get; } = new List<MediaIndexEntry>();

        public void Scan() {
            Entries.Clear();
            entriesByParentId.Clear();

            int i = 0;
            Scan(-1, ref i, new DirectoryInfo(Folder));
        }

        private void Scan(int parentId, ref int i, DirectoryInfo info) {
            int folderId = i;
            MediaIndexEntry directoryEntry = AddDirectoryEntry(parentId, i++, info);

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
                                 file.Tag.Artists?.Length > 0 ? string.Join('/', file.Tag.Artists): null;
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


        private MediaIndexEntry AddDirectoryEntry(int parentId, int index, FileSystemInfo info) {
            return AddEntry(parentId, index, info.Name, info.FullName, isFolder: true);
        }

        private MediaIndexEntry AddEntry(
            int parentId, int id, string name, string path, bool isFolder = false,
            int? trackNumber = null, string artist = null, TimeSpan? duration = null) {

            var entry = new MediaIndexEntry {
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

            return entry;
        }
    }
}
