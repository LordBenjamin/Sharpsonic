namespace Auricular.DataTransfer {
    public class DirectoryListingResponse {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ParentId { get; set; }
        public DirectoryListingItem[]? Items { get; set; }
    }
}
