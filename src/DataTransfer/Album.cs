namespace Auricular.DataTransfer {
    public class Album {
        public string Title { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Artist { get; set; }
        public bool IsDir { get; set; }
        public string CoverArt { get; set; }
    }
}
