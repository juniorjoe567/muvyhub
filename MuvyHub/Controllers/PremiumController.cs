using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuvyHub.Data;
using MuvyHub.Models;
using MuvyHub.Services;
using PagedList.Core;
using System.Linq;
using System.Threading.Tasks;

namespace MuvyHub.Controllers
{
    [Authorize]
    public class PremiumController : Controller
    {
        private readonly WasabiService _wasabiService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public PremiumController(WasabiService wasabiService, ApplicationDbContext context, UserManager<AppUser> userManager, IHttpClientFactory httpClientFactory)
        {
            _wasabiService = wasabiService;
            _context = context;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Categories()
        {
            var categoriesWithCount = await _wasabiService.ListCategoriesWithCountAsync();
            return View(categoriesWithCount);
        }

        [Authorize(Policy = "PremiumAccess")]
        public async Task<IActionResult> ViewCategory(string category, int page = 1, int pageSize = 15)
        {
            if (string.IsNullOrEmpty(category))
            {
                return BadRequest("A category is required.");
            }

            var postItems = await _wasabiService.ListPostsInCategoryAsync(category);

            var viewModel = new ViewCategoryViewModel
            {
                Category = category,
                Posts = postItems.AsQueryable().ToPagedList(page, pageSize)
            };

            return View(viewModel);
        }

        [Authorize(Policy = "PremiumAccess")]
        public async Task<IActionResult> ViewPost(string wasabiKey)
        {
            if (string.IsNullOrEmpty(wasabiKey))
            {
                return BadRequest("A post key is required.");
            }

            var job = await _context.UploadJobs.FirstOrDefaultAsync(j => j.WasabiKey == wasabiKey && j.IsSuccessful);
            if (job == null)
            {
                return NotFound();
            }

            job.ViewCount++;
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            var canDownload = currentUser?.CanDownload ?? false;

            string? downloadUrl = null;

            if (canDownload)
            {
                downloadUrl = _wasabiService.GetPresignedUrl(wasabiKey);
            }

            var allPostsInCategory = await _wasabiService.ListPostsInCategoryAsync(job.Folder);
            var currentPost = allPostsInCategory.First(p => p.WasabiKey == wasabiKey);
            var relatedPosts = allPostsInCategory.Where(p => p.WasabiKey != wasabiKey).ToList();

            var viewModel = new ViewPostViewModel
            {
                Post = currentPost,
                RelatedPosts = relatedPosts,
                CanDownload = canDownload,
                DirectDownloadUrl = downloadUrl
            };

            return View(viewModel);
        }

        [Authorize(Policy = "PremiumAccess")]
        [HttpGet("Premium/Stream/{id}")]
        public IActionResult Stream(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var decodedKey = System.Net.WebUtility.UrlDecode(id);
            var signedUrl = _wasabiService.GetPresignedUrl(decodedKey);

            if (string.IsNullOrEmpty(signedUrl))
            {
                return NotFound();
            }

            return Redirect(signedUrl);
        }

        [Authorize(Policy = "PremiumAccess")]
        [HttpGet]
        public async Task<IActionResult> Download(string videoKey)
        {
            if (string.IsNullOrEmpty(videoKey)) return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user?.CanDownload != true) return Forbid();

            var job = await _context.UploadJobs.AsNoTracking().FirstOrDefaultAsync(j => j.WasabiKey == videoKey);
            var friendlyFileName = job?.OriginalFileName ?? "video.mp4";

            var wasabiUrl = _wasabiService.GetPresignedUrl(videoKey);
            if (string.IsNullOrEmpty(wasabiUrl)) return NotFound();

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(wasabiUrl, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return new StatusCodeResult((int)response.StatusCode);
            }

            var stream = await response.Content.ReadAsStreamAsync();

            return File(stream, "application/octet-stream", friendlyFileName);
        }

        [HttpPost]
        [Authorize(Policy = "PremiumAccess")]
        public async Task<IActionResult> IncrementViewCount([FromBody] string wasabiKey)
        {
            if (string.IsNullOrEmpty(wasabiKey))
            {
                return BadRequest();
            }

            var job = await _context.UploadJobs.FirstOrDefaultAsync(j => j.WasabiKey == wasabiKey);
            if (job != null)
            {
                job.ViewCount++;
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }
    }
}
