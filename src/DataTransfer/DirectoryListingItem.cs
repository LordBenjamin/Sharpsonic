using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auricular.DataTransfer {
    public class DirectoryListingItem {
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public bool IsDir { get; set; }
        public int Track { get; set; }
        public bool TrackSpecified { get; set; }
        public int? CoverArt { get; set; }
        public int Duration { get; set; }
        public bool DurationSpecified { get; set; }
        public string? Path { get; set; }
    }
}
