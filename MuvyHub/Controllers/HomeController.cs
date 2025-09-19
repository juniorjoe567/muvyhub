using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuvyHub.Models;
using MuvyHub.Services;
using PagedList.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MuvyHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly TmdbService _tmdbService;
        private readonly WasabiService _wasabiService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(
            TmdbService tmdbService,
            WasabiService wasabiService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _tmdbService = tmdbService;
            _wasabiService = wasabiService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string categoryFilter, string searchQuery, string sortBy = "newest", int page = 1, int pageSize = 30)
        {
            var viewModel = new HomeViewModel();

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    viewModel.ExpiryDate = user.ExpiryDate;
                    viewModel.CanDownload = user.CanDownload;
                }

                var pagedResult = await _wasabiService.ListAllPostsAsync(
                         sortBy, categoryFilter, searchQuery, page, pageSize);

                viewModel.AllPosts = new StaticPagedList<PostItem>(
                    pagedResult.Items, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

                viewModel.CategoryFilter = categoryFilter;
                viewModel.SearchQuery = searchQuery;
                viewModel.SortBy = sortBy;

                var categories = await _wasabiService.ListCategoriesWithCountAsync();
                viewModel.CategoryFilterOptions = categories
                    .Select(c => new SelectListItem { Value = c.Name, Text = $"{c.Name} ({c.VideoCount})" })
                    .ToList();
            }
            else
            {
                viewModel.LatestPremiumVideos = await _wasabiService.ListLatestPremiumPostsAsync(120);

                var popularMovies = await _tmdbService.GetPopularMoviesAsync();
                viewModel.CarouselMovies = popularMovies?.Results
                    .Where(m => !string.IsNullOrEmpty(m.BackdropPath))
                    .OrderBy(m => Guid.NewGuid())
                    .Take(5).ToList() ?? new List<TmdbResult>();
            }

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
