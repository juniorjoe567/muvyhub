using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList.Core;

namespace MuvyHub.Models
{
    public class HomeViewModel
    {
        public List<PostItem> LatestPremiumVideos { get; set; } = new List<PostItem>();
        public List<TmdbResult> CarouselMovies { get; set; } = new List<TmdbResult>();

        public DateTime? ExpiryDate { get; set; }

        public IPagedList<PostItem>? AllPosts { get; set; }
        public string? CategoryFilter { get; set; }
        public string? SearchQuery { get; set; }
        public string SortBy { get; set; } = "newest";

        public List<SelectListItem> CategoryFilterOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "newest", Text = "Newest" },
            new SelectListItem { Value = "oldest", Text = "Oldest" },
            new SelectListItem { Value = "mostviewed", Text = "Most Viewed" }
        };

        public List<TmdbResult> PopularMovies { get; set; } = new List<TmdbResult>();
        public List<TmdbResult> TopRatedMovies { get; set; } = new List<TmdbResult>();
        public List<TmdbResult> PopularTvShows { get; set; } = new List<TmdbResult>();
        public List<TmdbResult> TopRatedTvShows { get; set; } = new List<TmdbResult>();
        public bool CanDownload { get; set; }
    }
}
