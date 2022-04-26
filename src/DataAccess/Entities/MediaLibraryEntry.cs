using System;
using System.Collections.Generic;
using System.IO;

namespace Auricular.DataAccess.Entities {
    public class MediaLibraryEntry : IEquatable<MediaLibraryEntry> {
        public int Id { get; set; }
        public string Artist { get; set; }
        public bool IsFolder { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }

        public string Path { get; set; }
        public int? TrackNumber { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime AddedUtc { get; set; }
        public DateTime? LastPlayedUtc { get; set; }

        public Stream OpenReadStream() {
            if (IsFolder) {
                throw new InvalidOperationException("Can't open a folder for reading.");
            }

            return File.OpenRead(Path);
        }

        public MediaLibraryEntry Copy() {
            return new MediaLibraryEntry {
                Id = Id,
                Artist = Artist,
                IsFolder = IsFolder,
                Name = Name,
                ParentId = ParentId,
                Path = Path,
                TrackNumber = TrackNumber,
                Duration = Duration,
                AddedUtc = AddedUtc,
                LastPlayedUtc = LastPlayedUtc,
            };
        }

        public override bool Equals(object obj) {
            return Equals(obj as MediaLibraryEntry);
        }

        public bool Equals(MediaLibraryEntry other) {
            if(other == null) {
                return false;
            }

            return EqualityComparer<int>.Default.Equals(Id, other.Id) &&
                EqualityComparer<string>.Default.Equals(Artist, other.Artist) &&
                EqualityComparer<bool>.Default.Equals(IsFolder, other.IsFolder) &&
                EqualityComparer<string>.Default.Equals(Name, other.Name) &&
                EqualityComparer<int>.Default.Equals(ParentId, other.ParentId) &&
                EqualityComparer<string>.Default.Equals(Path, other.Path) &&
                EqualityComparer<int?>.Default.Equals(TrackNumber, other.TrackNumber) &&
                EqualityComparer<TimeSpan?>.Default.Equals(Duration, other.Duration) &&
                EqualityComparer<DateTime>.Default.Equals(AddedUtc, other.AddedUtc) &&
                EqualityComparer<DateTime?>.Default.Equals(LastPlayedUtc, other.LastPlayedUtc);
        }

        public override int GetHashCode() {
            return (Id, Artist, IsFolder, Name, ParentId, Path, TrackNumber, Duration, AddedUtc, LastPlayedUtc).GetHashCode();
        }
    }
}