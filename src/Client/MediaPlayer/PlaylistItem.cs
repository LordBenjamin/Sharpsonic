using System;

namespace Auricular.Client.MediaPlayer {
    public class PlaylistItem : IEquatable<PlaylistItem> {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public string? Album { get; set; }
        public int Track { get; set; }
        public bool TrackSpecified { get; set; }
        public int? CoverArt { get; set; }
        public TimeSpan Duration { get; set; }
        public bool DurationSpecified { get; set; }

        public override bool Equals(object? obj) {
            return obj is PlaylistItem item && Equals(item);
        }

        public bool Equals(PlaylistItem? other) {
            if (other == null) {
                return false;
            }

            return Id == other.Id &&
                   ParentId == other.ParentId &&
                   Artist == other.Artist &&
                   Title == other.Title &&
                   Album == other.Album &&
                   Track == other.Track &&
                   TrackSpecified == other.TrackSpecified &&
                   CoverArt == other.CoverArt &&
                   Duration.Equals(other.Duration) &&
                   DurationSpecified == other.DurationSpecified;
        }

        public override int GetHashCode() {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(ParentId);
            hash.Add(Artist);
            hash.Add(Title);
            hash.Add(Album);
            hash.Add(Track);
            hash.Add(TrackSpecified);
            hash.Add(CoverArt);
            hash.Add(Duration);
            hash.Add(DurationSpecified);
            return hash.ToHashCode();
        }
    }
}