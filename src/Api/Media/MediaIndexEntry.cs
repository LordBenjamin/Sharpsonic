using System;
using System.IO;

namespace Api.Media {
    public class MediaIndexEntry {
        public int Id { get; set; }
        public bool IsFolder { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }

        public string Path { get; set; }
        public int? TrackNumber { get; set; }

        internal Stream OpenReadStream() {
            if (IsFolder) {
                throw new InvalidOperationException("Can't open a folder for reading.");
            }

            return File.OpenRead(Path);
        }
    }
}