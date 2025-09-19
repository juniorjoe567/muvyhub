using System.Collections.Generic;

namespace MuvyHub.Models
{
    public class PlayViewModel
    {
        public string CurrentVideoKey { get; set; } = string.Empty;
        public string CurrentVideoTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<VideoItem> RelatedVideos { get; set; } = new List<VideoItem>();
        public bool CanDownload { get; set; }
        public string? DirectDownloadUrl { get; set; }


    }
}
