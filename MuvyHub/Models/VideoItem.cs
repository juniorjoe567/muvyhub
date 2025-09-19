using System;

namespace MuvyHub.Models
{
    public class VideoItem
    {
        public string Title { get; set; } = string.Empty;
        public string VideoKey { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTime LastModified { get; set; }
        public TimeSpan Duration { get; set; }

        public int ViewCount { get; set; }
    }
}
