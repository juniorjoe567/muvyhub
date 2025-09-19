using Microsoft.AspNetCore.Identity;
using System;

namespace MuvyHub.Models
{
    public class AppUser : IdentityUser
    {
        public bool IsActive { get; set; } = false;
        public DateTime? ActivationDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public bool CanDownload { get; set; } = false;
    }
}
