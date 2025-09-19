using System;
using System.Collections.Generic;

namespace MuvyHub.Models
{
    public class PostItem
    {
        public string PostType { get; set; } = "Video";
        public string WasabiKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();

        public string? ThumbnailUrl { get; set; }
        public DateTime LastModified { get; set; }
        public int ViewCount { get; set; }
    }
}
