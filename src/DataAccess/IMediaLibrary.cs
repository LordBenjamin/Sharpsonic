using Auricular.DataAccess.Entities;
using System.Collections.Generic;

namespace Auricular.DataAccess
{
    public interface IMediaLibrary
    {
        int AddEntry(MediaLibraryEntry entry);
        IEnumerable<MediaLibraryEntry> GetAllEntries();
        IEnumerable<MediaLibraryEntry> GetChildEntries(int id);
        IEnumerable<MediaLibraryEntry> GetChildFiles(int id);
        IEnumerable<MediaLibraryEntry> GetChildFolders(int id);
        MediaLibraryEntry GetEntry(int id);
        MediaLibraryEntry GetEntryByPath(string fullName);
        MediaLibraryEntry GetFile(int id);
        long GetFileCount();
        MediaLibraryEntry GetFolder(int id);
        IEnumerable<MediaLibraryEntry> GetNonRootFolders();
        MediaLibraryEntry GetRootFolderFor(int id);
        MediaLibraryEntry GetRootFolderFor(MediaLibraryEntry dir);
        IEnumerable<MediaLibraryEntry> GetRootFolders();
        void RemoveEntry(MediaLibraryEntry entry);
        void UpdateLastPlayed(int id);
        void UpdateEntry(MediaLibraryEntry directoryEntry);
    }
}