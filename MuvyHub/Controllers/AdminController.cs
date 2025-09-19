// Located in: /Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuvyHub.Models;
using MuvyHub.Services;
using PagedList.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using MuvyHub.Data;

namespace MuvyHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly WasabiService _wasabiService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly UploadTrackerService _uploadTrackerService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AdminController(
            UserManager<AppUser> userManager,
            WasabiService wasabiService,
            IBackgroundJobClient backgroundJobClient,
            UploadTrackerService uploadTrackerService,
            ApplicationDbContext context,
            IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _wasabiService = wasabiService;
            _backgroundJobClient = backgroundJobClient;
            _uploadTrackerService = uploadTrackerService;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDownloadPermission(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.CanDownload = !user.CanDownload;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(ManageUsers));
        }
        public IActionResult ManageUsers(string searchQuery, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(u => u.Email.Contains(searchQuery));
            }

            var users = query.OrderBy(u => u.Email).ToPagedList(page, pageSize);

            var model = new ManageUsersViewModel
            {
                Users = users,
                SearchQuery = searchQuery
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult Uploads(int page = 1, int pageSize = 20)
        {
            var jobs = _context.UploadJobs
                .AsNoTracking()
                .OrderByDescending(j => j.StartTime)
                .ToPagedList(page, pageSize);
            return View(jobs);
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var categories = await _wasabiService.ListCategoriesWithCountAsync();
            var model = new UploadViewModel
            {
                Categories = categories.Select(c => c.Name).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            if (model.PostType == "Video")
            {
                if (model.Video == null) return Json(new { success = false, message = "A video file is required for a video post." });
                return await HandleVideoUpload(model);
            }
            else if (model.PostType == "Image")
            {
                if (model.Images == null || !model.Images.Any()) return Json(new { success = false, message = "At least one image is required for an image gallery post." });
                return await HandleImageUpload(model);
            }

            return Json(new { success = false, message = "Invalid post type." });
        }

        private async Task<IActionResult> HandleVideoUpload(UploadViewModel model)
        {
            try
            {
                var tempVideoPath = Path.GetTempFileName();
                using (var stream = new FileStream(tempVideoPath, FileMode.Create)) { await model.Video.CopyToAsync(stream); }

                string videoKey = await _wasabiService.GenerateUniqueFileNameAsync(model.SelectedCategory, ".mp4");
                string thumbnailKey = videoKey.Replace(".mp4", ".png");

                var jobId = _backgroundJobClient.Enqueue<IUploadJobService>(service =>
                    service.ProcessVideoUpload(tempVideoPath, videoKey, thumbnailKey, model.Description, null)
                );

                await _uploadTrackerService.CreateJobAsync(jobId, videoKey, model.Video.FileName, model.SelectedCategory, "Video", model.Description);

                var uploadsUrl = Url.Action("Uploads");
                return Json(new { success = true, message = $"Video post '{model.Video.FileName}' has been queued.", viewUrl = uploadsUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An unexpected error occurred: {ex.Message}" });
            }
        }

        private async Task<IActionResult> HandleImageUpload(UploadViewModel model)
        {
            try
            {
                var tempImagePaths = new List<string>();
                foreach (var image in model.Images)
                {
                    var tempPath = Path.GetTempFileName();
                    using (var stream = new FileStream(tempPath, FileMode.Create)) { await image.CopyToAsync(stream); }
                    tempImagePaths.Add(tempPath);
                }

                var primaryKey = await _wasabiService.GenerateUniqueFileNameAsync(model.SelectedCategory, "_post");

                var jobId = _backgroundJobClient.Enqueue<IUploadJobService>(service =>
                    service.ProcessImagePostUpload(tempImagePaths, model.SelectedCategory, primaryKey, model.Description, null)
                );

                await _uploadTrackerService.CreateJobAsync(jobId, primaryKey, model.Images.First().FileName, model.SelectedCategory, "Image", model.Description);

                var uploadsUrl = Url.Action("Uploads");
                return Json(new { success = true, message = "Image gallery post has been queued.", viewUrl = uploadsUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An unexpected error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ManageUsers));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                if (user.ExpiryDate == null || user.ExpiryDate < DateTime.UtcNow)
                {
                    user.ActivationDate = DateTime.UtcNow;
                    user.ExpiryDate = DateTime.UtcNow.AddDays(30);
                }
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExpiry(string id, int days)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var baseDate = (user.ExpiryDate.HasValue && user.ExpiryDate.Value > DateTime.UtcNow)
                    ? user.ExpiryDate.Value
                    : DateTime.UtcNow;

                user.ExpiryDate = baseDate.AddDays(days);

                if (days > 0)
                {
                    user.IsActive = true;
                    if (!user.ActivationDate.HasValue)
                    {
                        user.ActivationDate = DateTime.UtcNow;
                    }
                }

                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(string wasabiKey)
        {
            if (string.IsNullOrEmpty(wasabiKey)) return BadRequest();

            var job = await _context.UploadJobs.FirstOrDefaultAsync(j => j.WasabiKey == wasabiKey);
            if (job != null)
            {
                var wasabiSuccess = await _wasabiService.DeletePostFilesAsync(job);

                if (wasabiSuccess)
                {
                    _context.UploadJobs.Remove(job);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully deleted post: {job.OriginalFileName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete files from storage. The database record was not removed.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find the post to delete.";
            }

            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index", "Home"));
        }
    }
}
