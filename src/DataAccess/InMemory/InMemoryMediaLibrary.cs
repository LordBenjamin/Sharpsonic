using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sharpsonic.DataAccess.Entities;

namespace Sharpsonic.DataAccess.InMemory {

    public class InMemoryMediaLibrary : IMediaLibrary
    {

        private readonly UniqueIndex<int, MediaLibraryEntry> entriesById;
        private readonly UniqueIndex<string, MediaLibraryEntry> entriesByPath;
        private readonly NonUniqueIndex<int, MediaLibraryEntry> entriesByParentId;
        private readonly NonUniqueIndex<bool, MediaLibraryEntry> entriesByIsFolder;

        private int identity = -1;

        public InMemoryMediaLibrary(string folder)
        {

            entriesById = new UniqueIndex<int, MediaLibraryEntry>(v => v.Id);
            entriesByPath = new UniqueIndex<string, MediaLibraryEntry>(v => v.Path);
            entriesByParentId = new NonUniqueIndex<int, MediaLibraryEntry>(v => v.ParentId);
            entriesByIsFolder = new NonUniqueIndex<bool, MediaLibraryEntry>(v => v.IsFolder);

            Folder = folder;
        }

        public string Folder { get; }

        private List<MediaLibraryEntry> Entries { get; } = new List<MediaLibraryEntry>();

        public int AddEntry(MediaLibraryEntry entry)
        {
            entry.Id = Interlocked.Increment(ref identity);

            Entries.Add(entry);
            entriesById.Add(entry);
            entriesByPath.Add(entry);
            entriesByIsFolder.Add(entry);
            entriesByParentId.Add(entry);

            return entry.Id;
        }

        public IEnumerable<MediaLibraryEntry> GetNonRootFolders()
        {
            return Entries
                .Where(i => i.ParentId >= 0)
                .Where(i => i.IsFolder);
        }

        public IEnumerable<MediaLibraryEntry> GetRootFolders()
        {
            return entriesByParentId.Get(-1)
                .Where(i => i.IsFolder);
        }

        public IEnumerable<MediaLibraryEntry> GetAllEntries()
        {
            return Entries.AsReadOnly();
        }

        public MediaLibraryEntry GetEntry(int id)
        {
            return entriesById.Get(id);
        }

        public MediaLibraryEntry GetFolder(int id)
        {
            MediaLibraryEntry item = entriesById.Get(id);
            return item.IsFolder ? item : null;
        }

        public MediaLibraryEntry GetFile(int id)
        {
            MediaLibraryEntry item = entriesById.Get(id);
            return item.IsFolder ? null : item;
        }

        public MediaLibraryEntry GetRootFolderFor(int id)
        {
            return GetRootFolderFor(GetFolder(id));
        }

        public MediaLibraryEntry GetRootFolderFor(MediaLibraryEntry dir)
        {
            MediaLibraryEntry rootDir = dir;
            while (rootDir.ParentId >= 0)
            {
                rootDir = entriesById.Get(rootDir.ParentId);
            }

            return rootDir;
        }

        public IEnumerable<MediaLibraryEntry> GetChildEntries(int id)
        {
            return entriesByParentId.Get(id);
        }

        public IEnumerable<MediaLibraryEntry> GetChildFolders(int id)
        {
            return GetChildEntries(id)
                .Where(i => i.IsFolder);
        }

        public IEnumerable<MediaLibraryEntry> GetChildFiles(int id)
        {
            return GetChildEntries(id)
                .Where(i => !i.IsFolder);
        }

        public long GetFileCount()
        {
            return entriesByIsFolder.Get(false)?.Count ?? 0;
        }

        public MediaLibraryEntry GetEntryByPath(string fullName)
        {
            return entriesByPath.Get(fullName);
        }

        public void UpdateLastPlayed(int id)
        {
            DateTime now = DateTime.UtcNow;
            MediaLibraryEntry entry = GetEntry(id);

            entry.LastPlayedUtc = now;

            // If this is a file, also update the parent folder
            if (!entry.IsFolder)
            {
                entry = GetEntry(entry.ParentId);

                entry.LastPlayedUtc = now;
            }
        }

        public void RemoveEntry(MediaLibraryEntry entry)
        {
            Entries.Remove(entry);
            entriesById.Remove(entry);
            entriesByPath.Remove(entry);
            entriesByParentId.Remove(entry);
            entriesByIsFolder.Remove(entry);
        }

        public void UpdateEntry(MediaLibraryEntry entry) {
            RemoveEntry(entry);
            AddEntry(entry);
        }
    }
}
