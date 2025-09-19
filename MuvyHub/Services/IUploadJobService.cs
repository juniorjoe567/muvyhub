// Located in: /Services/IUploadJobService.cs
using Hangfire;
using Hangfire.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuvyHub.Services
{
    public interface IUploadJobService
    {
        [JobDisplayName("Processing Video Post: {1}")]
        Task ProcessVideoUpload(string tempVideoPath, string videoKey, string thumbnailKey, string? description, PerformContext? context);

        [JobDisplayName("Processing Image Gallery Post: {2}")]
        Task ProcessImagePostUpload(List<string> tempImagePaths, string category, string primaryKey, string? description, PerformContext? context);
    }
}
