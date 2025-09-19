using Microsoft.EntityFrameworkCore;
using MuvyHub.Data;
using MuvyHub.Models;
using System;
using System.Threading.Tasks;

namespace MuvyHub.Services
{
    public class UploadTrackerService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public UploadTrackerService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<UploadJob> CreateJobAsync(string hangfireJobId, string wasabiKey, string originalFileName, string folder, string postType, string? description)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var job = new UploadJob
            {
                HangfireJobId = hangfireJobId,
                WasabiKey = wasabiKey,
                OriginalFileName = originalFileName,
                Folder = folder,
                PostType = postType,
                Description = description,
                Status = "Queued",
                StartTime = DateTime.UtcNow
            };
            context.UploadJobs.Add(job);
            await context.SaveChangesAsync();
            return job;
        }

        public async Task UpdateJobImageKeysAsync(string hangfireJobId, string imageKeysJson)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var job = await context.UploadJobs.FirstOrDefaultAsync(j => j.HangfireJobId == hangfireJobId);
            if (job != null)
            {
                job.ImageKeysJson = imageKeysJson;
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateJobStatusAsync(string hangfireJobId, string status, string? errorMessage = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var job = await context.UploadJobs.FirstOrDefaultAsync(j => j.HangfireJobId == hangfireJobId);
            if (job != null)
            {
                job.Status = status;
                if (status == "Completed" || status == "Failed")
                {
                    job.CompletionTime = DateTime.UtcNow;
                    job.IsSuccessful = (status == "Completed");
                    job.ErrorMessage = errorMessage;
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateJobDurationAsync(string hangfireJobId, TimeSpan duration)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var job = await context.UploadJobs.FirstOrDefaultAsync(j => j.HangfireJobId == hangfireJobId);
            if (job != null)
            {
                job.Duration = duration;
                await context.SaveChangesAsync();
            }
        }
    }
}
