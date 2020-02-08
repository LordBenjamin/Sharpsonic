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

            var fileSystemEntries = info.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

            HashSet<string> albumNames = new HashSet<string>();

            foreach (FileSystemInfo info2 in fileSystemEntries) {
                if (info2 is DirectoryInfo directoryInfo) {
                    Scan(folderId, ref i, directoryInfo);
                }
                else if (info2.Name.EndsWith(".mp3")) {
                    string title;
                    int? trackNumber;

                    using (var file = TagLib.File.Create(info2.FullName)) {
                        title = file.Tag?.Title ??
                            Path.GetFileNameWithoutExtension(info2.Name);

                        trackNumber = (int)file.Tag?.Track;

                        string album = file.Tag?.Album?.Trim();

                        if (!string.IsNullOrWhiteSpace(album)) {
                            albumNames.Add(album);
                        }
                    }

                    AddEntry(folderId, i++,
                        name: title,
                        path: info2.FullName,
                        trackNumber: trackNumber);
                }
            }

            if (albumNames.Count == 1) {
                directoryEntry.Name = albumNames.Single();
            }
        }


        private MediaIndexEntry AddDirectoryEntry(int parentId, int index, FileSystemInfo info) {
            return AddEntry(parentId, index, info.Name, info.FullName, isFolder: true);
        }

        private MediaIndexEntry AddEntry(int parentId, int id, string name, string path, bool isFolder = false, int? trackNumber = null) {
            var entry = new MediaIndexEntry {
                Id = id,
                ParentId = parentId,
                IsFolder = isFolder,
                Name = name,
                Path = path,
                TrackNumber = trackNumber,
            };

            Entries.Add(entry);

            return entry;
        }
    }
}
