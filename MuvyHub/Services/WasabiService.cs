using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using MuvyHub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using Amazon.S3.Transfer;
using System.Threading;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using MuvyHub.Data;
using System.Text.Json;
using System.Net;

namespace MuvyHub.Services
{
    public class WasabiService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public WasabiService(IConfiguration configuration, IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            var accessKey = configuration["Wasabi:AccessKey"];
            var secretKey = configuration["Wasabi:SecretKey"];
            var serviceUrl = configuration["Wasabi:ServiceUrl"];
            _bucketName = configuration["Wasabi:BucketName"];

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(_bucketName))
            {
                throw new ArgumentException("Wasabi configuration is missing from appsettings.json");
            }

            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            var config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                Timeout = TimeSpan.FromHours(2),
                MaxErrorRetry = 5
            };

            _s3Client = new AmazonS3Client(credentials, config);
        }

        public string GetPresignedUrl(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };
            return _s3Client.GetPreSignedURL(request);
        }

        public async Task<List<CategoryItemViewModel>> ListCategoriesWithCountAsync()
        {
            var categoriesWithCount = new List<CategoryItemViewModel>();
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Delimiter = "/"
                };
                var rootFolders = (await _s3Client.ListObjectsV2Async(request)).CommonPrefixes;

                foreach (var folder in rootFolders)
                {
                    var countRequest = new ListObjectsV2Request
                    {
                        BucketName = _bucketName,
                        Prefix = folder
                    };
                    int videoCount = 0;
                    ListObjectsV2Response countResponse;
                    do
                    {
                        countResponse = await _s3Client.ListObjectsV2Async(countRequest);
                        videoCount += countResponse.S3Objects.Count(o => o.Key.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));
                        countRequest.ContinuationToken = countResponse.NextContinuationToken;
                    } while (countResponse.IsTruncated);

                    categoriesWithCount.Add(new CategoryItemViewModel
                    {
                        Name = folder.TrimEnd('/'),
                        VideoCount = videoCount
                    });
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when listing categories with count.");
            }
            return categoriesWithCount;
        }

        public async Task<string> GenerateUniqueFileNameAsync(string category, string extension)
        {
            var today = DateTime.UtcNow;
            var datePrefix = $"{today:yyyy_M_d}";

            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);

            var fileName = $"{category}_{datePrefix}_{uniqueId}{extension}";
            var fullKey = $"{category}/{fileName}";

            return fullKey;
        }
        public async Task<bool> UploadFileFromPathAsync(string filePath, string key, string contentType, IProgress<double>? progress = null)
        {
            try
            {
                var transferUtilityConfig = new TransferUtilityConfig
                {
                    MinSizeBeforePartUpload = 15 * 1024 * 1024,
                };
                using var transferUtility = new TransferUtility(_s3Client, transferUtilityConfig);

                var request = new TransferUtilityUploadRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    FilePath = filePath,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.Private
                };

                if (progress != null)
                {
                    request.UploadProgressEvent += (sender, args) =>
                    {
                        progress.Report(args.PercentDone);
                    };
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromHours(1));
                await transferUtility.UploadAsync(request, cts.Token);

                return true;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error uploading from path to Wasabi: {e.Message}");
                return false;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Upload was cancelled due to timeout.");
                return false;
            }
        }

        public async Task<List<PostItem>> ListPostsInCategoryAsync(string category, string sortBy = "newest")
        {
            var postItems = new List<PostItem>();
            try
            {
                var s3Request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = $"{category}/"
                };
                var s3Objects = (await _s3Client.ListObjectsV2Async(s3Request)).S3Objects;
                var allFilesSet = new HashSet<string>(s3Objects.Select(o => o.Key));

                await using var dbContext = await _contextFactory.CreateDbContextAsync();
                var query = dbContext.UploadJobs
                    .Where(j => j.Folder == category && j.IsSuccessful);

                switch (sortBy.ToLower())
                {
                    case "oldest":
                        query = query.OrderBy(j => j.StartTime);
                        break;
                    case "mostviewed":
                        query = query.OrderByDescending(j => j.ViewCount);
                        break;
                    default:
                        query = query.OrderByDescending(j => j.StartTime);
                        break;
                }

                var jobs = await query.ToListAsync();

                foreach (var job in jobs)
                {
                    var postItem = new PostItem
                    {
                        WasabiKey = job.WasabiKey,
                        Title = job.OriginalFileName,
                        Description = job.Description,
                        LastModified = job.StartTime,
                        ViewCount = job.ViewCount,
                        Duration = job.Duration,
                        PostType = job.PostType
                    };

                    if (job.PostType == "Image" && !string.IsNullOrEmpty(job.ImageKeysJson))
                    {
                        var imageKeys = JsonSerializer.Deserialize<List<string>>(job.ImageKeysJson);
                        if (imageKeys != null && imageKeys.Any() && allFilesSet.Contains(imageKeys.First()))
                        {
                            postItem.ThumbnailUrl = GetPresignedUrl(imageKeys.First());
                            postItem.ImageUrls = imageKeys.Select(GetPresignedUrl).ToList();
                        }
                    }
                    else
                    {
                        postItem.PostType = "Video";

                        var fileNameWithoutExtension = job.WasabiKey.Substring(0, job.WasabiKey.LastIndexOf('.'));
                        var thumbnailKeyPng = $"{fileNameWithoutExtension}.png";
                        var thumbnailKeyJpg = $"{fileNameWithoutExtension}.jpg";

                        string? finalThumbnailKey = null;
                        if (allFilesSet.Contains(thumbnailKeyPng))
                        {
                            finalThumbnailKey = thumbnailKeyPng;
                        }
                        else if (allFilesSet.Contains(thumbnailKeyJpg))
                        {
                            finalThumbnailKey = thumbnailKeyJpg;
                        }

                        if (finalThumbnailKey != null)
                        {
                            postItem.ThumbnailUrl = GetPresignedUrl(finalThumbnailKey);
                        }
                    }

                    postItems.Add(postItem);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error listing posts: {e.Message}");
            }
            return postItems;
        }

        public async Task<List<PostItem>> ListLatestPremiumPostsAsync(int totalCount = 15)
        {
            var allLatestPosts = new List<PostItem>();
            try
            {
                var categories = await ListCategoriesWithCountAsync();

                foreach (var category in categories)
                {
                    var postsInCategory = await ListPostsInCategoryAsync(category.Name);
                    allLatestPosts.AddRange(postsInCategory);
                }

                var random = new Random();
                return allLatestPosts.OrderByDescending(a=>a.LastModified).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error listing latest premium posts: {e.Message}");
                return new List<PostItem>();
            }
        }

        public async Task<PagedResult<PostItem>> ListAllPostsAsync(
    string sortBy = "newest",
    string? categoryFilter = null,
    string? searchQuery = null,
    int page = 1,
    int pageSize = 20)
        {
            var postItems = new List<PostItem>();

            await using var dbContext = await _contextFactory.CreateDbContextAsync();
            var query = dbContext.UploadJobs.Where(j => j.IsSuccessful);

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(j => j.Folder == categoryFilter);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(j =>
                    j.OriginalFileName.Contains(searchQuery) || j.Folder.Contains(searchQuery));
            }

            query = sortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(j => j.StartTime),
                "mostviewed" => query.OrderByDescending(j => j.ViewCount),
                _ => query.OrderByDescending(j => j.StartTime),
            };

            var totalCount = await query.CountAsync();

            var jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var job in jobs)
            {
                var postItem = new PostItem
                {
                    WasabiKey = job.WasabiKey,
                    Title = job.OriginalFileName,
                    Description = job.Description,
                    LastModified = job.StartTime,
                    ViewCount = job.ViewCount,
                    Duration = job.Duration,
                    PostType = job.PostType,
                    Category = job.Folder
                };

                if (job.PostType == "Image" && !string.IsNullOrEmpty(job.ImageKeysJson))
                {
                    var imageKeys = JsonSerializer.Deserialize<List<string>>(job.ImageKeysJson);
                    if (imageKeys is { Count: > 0 })
                    {
                        postItem.ImageUrls = imageKeys.Select(GetPresignedUrl).ToList();
                        postItem.ThumbnailUrl = GetPresignedUrl(imageKeys.First());
                    }
                }
                else
                {
                    postItem.PostType = "Video";

                    var baseKey = Path.ChangeExtension(job.WasabiKey, null);
                    var thumbnailKey = $"{baseKey}.png";

                    postItem.ThumbnailUrl = GetPresignedUrl(thumbnailKey);
                }

                postItems.Add(postItem);
            }

            return new PagedResult<PostItem>
            {
                Items = postItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private async Task<bool> FileExistsAsync(string key)
        {
            try
            {
                var metadata = await _s3Client.GetObjectMetadataAsync(_bucketName, key);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<bool> DeletePostFilesAsync(UploadJob job)
        {
            var keysToDelete = new List<KeyVersion>();

            if (job.PostType == "Video")
            {
                keysToDelete.Add(new KeyVersion { Key = job.WasabiKey });
                var thumbKeyPng = job.WasabiKey.Replace(".mp4", ".png");
                keysToDelete.Add(new KeyVersion { Key = thumbKeyPng });
                var thumbKeyJpg = job.WasabiKey.Replace(".mp4", ".jpg");
                keysToDelete.Add(new KeyVersion { Key = thumbKeyJpg });
            }
            else if (job.PostType == "Image" && !string.IsNullOrEmpty(job.ImageKeysJson))
            {
                var imageKeys = JsonSerializer.Deserialize<List<string>>(job.ImageKeysJson);
                if (imageKeys != null)
                {
                    keysToDelete.AddRange(imageKeys.Select(key => new KeyVersion { Key = key }));
                }
            }

            if (!keysToDelete.Any()) return true;

            try
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = keysToDelete
                };
                await _s3Client.DeleteObjectsAsync(deleteRequest);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting post files from Wasabi: {e.Message}");
                return false;
            }
        }

    }
}
