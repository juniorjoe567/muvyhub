using PagedList.Core;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuvyHub.Models
{
    public class ViewCategoryViewModel
    {
        public string Category { get; set; } = string.Empty;

        public string SortBy { get; set; } = "newest";
        public List<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "newest", Text = "Newest" },
            new SelectListItem { Value = "oldest", Text = "Oldest" },
            new SelectListItem { Value = "mostviewed", Text = "Most Viewed" }
        };

        public IPagedList<PostItem> Posts { get; set; } = new StaticPagedList<PostItem>(new List<PostItem>(), 1, 15, 0);
    }
}
