using System;
using System.ComponentModel.DataAnnotations;

namespace MuvyHub.Models
{
    public class UploadJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string HangfireJobId { get; set; } = string.Empty;

        [Required]
        public string WasabiKey { get; set; } = string.Empty;

        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        public string PostType { get; set; } = "Video";
        public string? Description { get; set; }
        public string? ImageKeysJson { get; set; }

        [Required]
        public string Folder { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Queued";

        public DateTime StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
        public int ViewCount { get; set; } = 0;
    }
}
