using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;
using Sharpsonic.Api.Settings;

namespace Sharpsonic.Api.Media.InMemory {

    public class InMemoryMediaLibrary {

        private readonly UniqueIndex<int, MediaLibraryEntry> entriesById;
        private readonly UniqueIndex<string, MediaLibraryEntry> entriesByPath;
        private readonly NonUniqueIndex<int, MediaLibraryEntry> entriesByParentId;
        private readonly NonUniqueIndex<bool, MediaLibraryEntry> entriesByIsFolder;

        private int identity = -1;

        public InMemoryMediaLibrary(IOptions<MediaLibrarySettings> settings)
            : this(settings.Value.SourceDirectory) {

            entriesById = new UniqueIndex<int, MediaLibraryEntry>(v => v.Id);
            entriesByPath = new UniqueIndex<string, MediaLibraryEntry>(v => v.Path);
            entriesByParentId = new NonUniqueIndex<int, MediaLibraryEntry>(v => v.ParentId);
            entriesByIsFolder = new NonUniqueIndex<bool, MediaLibraryEntry>(v => v.IsFolder);
        }

        public InMemoryMediaLibrary(string folder) {
            Folder = folder;
        }

        public string Folder { get; }

        private List<MediaLibraryEntry> Entries { get; } = new List<MediaLibraryEntry>();

        internal int AddEntry(MediaLibraryEntry entry) {
            entry.Id = Interlocked.Increment(ref identity);

            Entries.Add(entry);
            entriesById.Add(entry);
            entriesByPath.Add(entry);
            entriesByIsFolder.Add(entry);
            entriesByParentId.Add(entry);

            return entry.Id;
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

        internal IEnumerable<MediaLibraryEntry> GetAllEntries() {
            return Entries.AsReadOnly();
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

        internal MediaLibraryEntry GetEntryByPath(string fullName) {
            return entriesByPath.Get(fullName);
        }

        internal void UpdateLastPlayed(int id) {
            DateTime now = DateTime.UtcNow;
            MediaLibraryEntry entry = GetEntry(id);

            entry.LastPlayedUtc = now;

            // If this is a file, also update the parent folder
            if(!entry.IsFolder) {
                entry = GetEntry(entry.ParentId);

                entry.LastPlayedUtc = now;
            }
        }

        internal void RemoveEntry(MediaLibraryEntry entry) {
            Entries.Remove(entry);
            entriesById.Remove(entry);
            entriesByPath.Remove(entry);
            entriesByParentId.Remove(entry);
            entriesByIsFolder.Remove(entry);
        }
    }
}