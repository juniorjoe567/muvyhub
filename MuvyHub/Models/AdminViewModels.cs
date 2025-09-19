using Microsoft.AspNetCore.Http;
using PagedList.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuvyHub.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ManageUsersViewModel
    {
        public IPagedList<AppUser>? Users { get; set; }
        public string SearchQuery { get; set; }
    }

    public class UploadViewModel
    {
        [Required]
        [Display(Name = "Category")]
        public string SelectedCategory { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Post Type")]
        public string PostType { get; set; } = "Video";

        [Display(Name = "Description (optional)")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Display(Name = "Video File (.mp4)")]
        public IFormFile? Video { get; set; }

        [Display(Name = "Images (select one or more)")]
        public List<IFormFile>? Images { get; set; }

        public List<string> Categories { get; set; } = new List<string>();
    }
}
