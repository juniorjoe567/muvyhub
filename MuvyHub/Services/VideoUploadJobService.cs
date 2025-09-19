using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.SignalR;
using MuvyHub.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace MuvyHub.Services
{
    public class VideoUploadJobService : IUploadJobService
    {
        private readonly ILogger<VideoUploadJobService> _logger;
        private readonly WasabiService _wasabiService;
        private readonly UploadTrackerService _trackerService;
        private readonly IHubContext<ProgressHub> _hubContext;

        public VideoUploadJobService(
            ILogger<VideoUploadJobService> logger,
            WasabiService wasabiService,
            UploadTrackerService trackerService,
            IHubContext<ProgressHub> hubContext)
        {
            _logger = logger;
            _wasabiService = wasabiService;
            _trackerService = trackerService;
            _hubContext = hubContext;
        }

        public async Task ProcessVideoUpload(string tempVideoPath, string videoKey, string thumbnailKey, string? description, PerformContext? context)
        {
            var hangfireJobId = context?.BackgroundJob.Id ?? "N/A";
            var tempThumbPath = Path.ChangeExtension(Path.GetTempFileName(), ".png");

            try
            {
                await _trackerService.UpdateJobStatusAsync(hangfireJobId, "Processing");
                var mediaInfo = await FFmpeg.GetMediaInfo(tempVideoPath);
                await _trackerService.UpdateJobDurationAsync(hangfireJobId, mediaInfo.Duration);

                var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(tempVideoPath, tempThumbPath, TimeSpan.FromSeconds(8));
                await conversion.Start();

                await _wasabiService.UploadFileFromPathAsync(tempThumbPath, thumbnailKey, "image/png");
                await _wasabiService.UploadFileFromPathAsync(tempVideoPath, videoKey, "video/mp4");

                await _trackerService.UpdateJobStatusAsync(hangfireJobId, "Completed");
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveProgress", hangfireJobId, "Completed", 100);
            }
            catch (Exception ex)
            {
                await _trackerService.UpdateJobStatusAsync(hangfireJobId, "Failed", ex.Message);
                throw;
            }
            finally
            {
                if (File.Exists(tempVideoPath)) File.Delete(tempVideoPath);
                if (File.Exists(tempThumbPath)) File.Delete(tempThumbPath);
            }
        }

        public async Task ProcessImagePostUpload(List<string> tempImagePaths, string category, string primaryKey, string? description, PerformContext? context)
        {
            var hangfireJobId = context?.BackgroundJob.Id ?? "N/A";
            var uploadedImageKeys = new List<string>();

            try
            {
                await _trackerService.UpdateJobStatusAsync(hangfireJobId, "Processing");

                for (int i = 0; i < tempImagePaths.Count; i++)
                {
                    var tempPath = tempImagePaths[i];
                    var imageName = $"{Path.GetFileNameWithoutExtension(primaryKey)}_{i + 1}.jpg";
                    var imageKey = $"{category}/{imageName}";

                    await _wasabiService.UploadFileFromPathAsync(tempPath, imageKey, "image/jpeg");
                    uploadedImageKeys.Add(imageKey);
                }

                var imageKeysJson = JsonSerializer.Serialize(uploadedImageKeys);
                await _trackerService.UpdateJobImageKeysAsync(hangfireJobId, imageKeysJson);

                await _trackerService.UpdateJobStatusAsync(hangfireJobId, "Completed");
            }
            catch (Exception ex)
            {
                await _trackerService.UpdateJobStatusAsync(hangfireJobId, "Failed", ex.Message);
                throw;
            }
            finally
            {
                foreach (var path in tempImagePaths)
                {
                    if (File.Exists(path)) File.Delete(path);
                }
            }
        }
    }
}
