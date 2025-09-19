using Microsoft.AspNetCore.Mvc;
using MuvyHub.Models;
using MuvyHub.Services;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MuvyHub.Controllers
{
    public class MoviesController : Controller
    {
        private readonly TmdbService _tmdbService;

        public MoviesController(TmdbService tmdbService)
        {
            _tmdbService = tmdbService;
        }

        public async Task<IActionResult> Index()
        {
            var popularMoviesTask = _tmdbService.GetPopularMoviesAsync();
            var topRatedMoviesTask = _tmdbService.GetTopRatedMoviesAsync();
            var popularTvShowsTask = _tmdbService.GetPopularTvShowsAsync();
            var topRatedTvShowsTask = _tmdbService.GetTopRatedTvShowsAsync();

            await Task.WhenAll(popularMoviesTask, topRatedMoviesTask, popularTvShowsTask, topRatedTvShowsTask);

            var viewModel = new MoviesViewModel
            {
                PopularMovies = popularMoviesTask.Result?.Results ?? new List<TmdbResult>(),
                TopRatedMovies = topRatedMoviesTask.Result?.Results ?? new List<TmdbResult>(),
                PopularTvShows = popularTvShowsTask.Result?.Results ?? new List<TmdbResult>(),
                TopRatedTvShows = topRatedTvShowsTask.Result?.Results ?? new List<TmdbResult>()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id, string mediaType)
        {
            if (string.IsNullOrEmpty(mediaType) || (mediaType != "movie" && mediaType != "tv"))
            {
                return BadRequest("Invalid media type.");
            }

            var detailsTask = _tmdbService.GetDetailsAsync(mediaType, id);
            var videosTask = _tmdbService.GetVideosAsync(mediaType, id);

            await Task.WhenAll(detailsTask, videosTask);

            var details = detailsTask.Result;
            if (details == null)
            {
                return NotFound();
            }

            var videos = videosTask.Result;
            var trailer = videos?.Results
                .Where(v => v.Site == "YouTube" && v.Type == "Trailer" && v.Official)
                .OrderByDescending(v => v.PublishedAt)
                .FirstOrDefault();

            var viewModel = new DetailsViewModel
            {
                Details = details,
                TrailerKey = trailer?.Key
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return PartialView("_SearchResults", new List<TmdbResult>());
            }

            var searchResults = await _tmdbService.SearchAsync(query);
            var filteredResults = searchResults?.Results
                .Where(r => (r.MediaType == "movie" || r.MediaType == "tv") && !string.IsNullOrEmpty(r.PosterPath))
                .ToList() ?? new List<TmdbResult>();

            return PartialView("_SearchResults", filteredResults);
        }
    }
}
