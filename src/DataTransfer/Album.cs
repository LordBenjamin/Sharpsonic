namespace Auricular.DataTransfer {
    public class Album {
        public string? Title { get; set; }
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public string? Artist { get; set; }
        public bool IsDir { get; set; }
        public int? CoverArtId { get; set; }
    }
}
