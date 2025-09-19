using System.Collections.Generic;

namespace MuvyHub.Models
{
    public class ViewPostViewModel
    {
        public PostItem Post { get; set; } = new PostItem();
        public List<PostItem> RelatedPosts { get; set; } = new List<PostItem>();
        public bool CanDownload { get; set; }
        public string? DirectDownloadUrl { get; set; }


    }
}
