using System;
using System.Collections.Generic;
using System.Linq;
using Sharpsonic.DataAccess.Entities;

namespace Sharpsonic.DataAccess.Sqlite {
    public class SqliteMediaLibrary : IMediaLibrary {
        public int AddEntry(MediaLibraryEntry entry) {
            using (var context = new SqliteDbContext()) {
                context.Add(entry);
                context.SaveChanges();

                return entry.Id;
            }
        }

        public IEnumerable<MediaLibraryEntry> GetAllEntries() {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries.ToArray();
            }
        }

        public IEnumerable<MediaLibraryEntry> GetChildEntries(int id) {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.ParentId == id)
                    .ToArray();
            }
        }

        public IEnumerable<MediaLibraryEntry> GetChildFiles(int id) {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.ParentId == id)
                    .Where(e => !e.IsFolder)
                    .ToArray();
            }
        }

        public IEnumerable<MediaLibraryEntry> GetChildFolders(int id) {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.ParentId == id)
                    .Where(e => e.IsFolder)
                    .ToArray();
            }
        }

        public MediaLibraryEntry GetEntry(int id) {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.Id == id)
                    .SingleOrDefault();
            }
        }

        public MediaLibraryEntry GetEntryByPath(string fullName) {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.Path == fullName)
                    .SingleOrDefault();
            }
        }

        public long GetFileCount() {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => !e.IsFolder)
                    .Count();
            }
        }

        public MediaLibraryEntry GetFile(int id) {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.Id == id)
                    .Where(e => !e.IsFolder)
                    .SingleOrDefault();
            }
        }

        public MediaLibraryEntry GetFolder(int id) {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.Id == id)
                    .Where(e => e.IsFolder)
                    .SingleOrDefault();
            }
        }

        public IEnumerable<MediaLibraryEntry> GetNonRootFolders() {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.IsFolder)
                    .Where(e => e.ParentId >= 0)
                    .ToArray();
            }
        }

        public MediaLibraryEntry GetRootFolderFor(int id) {
            return GetRootFolderFor(GetFolder(id));
        }

        public MediaLibraryEntry GetRootFolderFor(MediaLibraryEntry dir) {
            if(dir == null) {
                return null;
            }

            using (var context = new SqliteDbContext()) {
                MediaLibraryEntry rootDir = dir;
                while (rootDir.ParentId >= 0) {
                    rootDir = context.LibraryEntries
                        .Where(e => e.Id == rootDir.ParentId)
                        .SingleOrDefault();
                }

                return rootDir;
            }
        }

        public IEnumerable<MediaLibraryEntry> GetRootFolders() {
            using (var context = new SqliteDbContext()) {
                return context.LibraryEntries
                    .Where(e => e.IsFolder)
                    .Where(e => e.ParentId == -1)
                    .ToArray();
            }
        }

        public void RemoveEntry(MediaLibraryEntry entry) {
            using (var context = new SqliteDbContext()) {
                context.Remove(entry);
                context.SaveChanges();
            }
        }

        public void UpdateLastPlayed(int id) {
            DateTime now = DateTime.UtcNow;

            using (var context = new SqliteDbContext()) {
                MediaLibraryEntry entry = context.LibraryEntries
                    .Where(e => e.Id == id)
                    .SingleOrDefault();


                entry.LastPlayedUtc = now;

                // If this is a file, also update the parent folder
                if (!entry.IsFolder) {
                    entry = context.LibraryEntries
                        .Where(e => e.Id == entry.ParentId)
                        .SingleOrDefault();

                    entry.LastPlayedUtc = now;
                }

                context.SaveChanges();
            }
        }

        public void UpdateEntry(MediaLibraryEntry entry) {
            using (var context = new SqliteDbContext()) {
                context.Update(entry);
                context.SaveChanges();
            }
        }
    }
}
