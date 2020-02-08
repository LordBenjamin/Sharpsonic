using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Media
{

    public class MediaIndex
    {
        private readonly Dictionary<int, MediaIndexEntry> entriesByParentId = new Dictionary<int, MediaIndexEntry>();

        public MediaIndex(string folder)
        {
            Folder = folder;
        }

        public string Folder { get; }

        public List<MediaIndexEntry> Entries { get; } = new List<MediaIndexEntry>();

        public void Scan()
        {
            Entries.Clear();
            entriesByParentId.Clear();

            int i = 0;
            Scan(-1, ref i, new DirectoryInfo(Folder));
        }

        private void Scan(int parentId, ref int i, DirectoryInfo info)
        {
            int folderId = i;
            AddEntry(parentId, i++, info);

            var fileSystemEntries = info.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

            foreach (FileSystemInfo info2 in fileSystemEntries)
            {
                if (info2 is DirectoryInfo directoryInfo)
                {
                    Scan(folderId, ref i, directoryInfo);
                }
                else if(info2.Name.EndsWith(".mp3"))
                {
                    AddEntry(folderId, i++, info2);
                }
            }
        }

        private void AddEntry(int parentId, int index, FileSystemInfo info)
        {
            Entries.Add(new MediaIndexEntry
            {
                Id = index,
                ParentId = parentId,
                IsFolder = info is DirectoryInfo,
                Name = info.Name,
                Path = info.FullName,
            });
        }
    }
}
