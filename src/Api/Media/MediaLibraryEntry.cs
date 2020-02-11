using System;
using System.IO;

namespace Sharpsonic.Api.Media {
    public class MediaLibraryEntry {
        public int Id { get; set; }
        public string Artist { get; set; }
        public bool IsFolder { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }

        public string Path { get; set; }
        public int? TrackNumber { get; set; }
        public TimeSpan? Duration { get; set; }

        internal Stream OpenReadStream() {
            if (IsFolder) {
                throw new InvalidOperationException("Can't open a folder for reading.");
            }

            return File.OpenRead(Path);
        }
    }
}