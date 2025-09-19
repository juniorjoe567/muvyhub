using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MuvyHub.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace MuvyHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "This account is inactive. Please contact support.");
                    return View(model);
                }
                if (user.ExpiryDate.HasValue && user.ExpiryDate.Value <= DateTime.Now)
                {
                    ModelState.AddModelError(string.Empty, "Your premium access has expired.");
                    return View(model);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);

                    var claims = new List<Claim>
                    {
                        new Claim("IsActive", user.IsActive.ToString()),
                        new Claim("ExpiryDate", user.ExpiryDate?.ToString("o") ?? "")
                    };

                    await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);

                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckAuthStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "Invalid login attempt." });
            }

            var securityStampFromPrincipal = User?.FindFirstValue("AspNet.Identity.SecurityStamp");
            var securityStampFromDb = await _userManager.GetSecurityStampAsync(user);

            if (securityStampFromPrincipal != securityStampFromDb)
            {
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "You have been signed out because your account was used on another device." });
            }

            if (!user.IsActive)
            {
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "This account is inactive. Please contact support." });
            }

            if (user.ExpiryDate.HasValue && user.ExpiryDate.Value <= DateTime.Now)
            {
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "Your premium access has expired." });
            }

            return Ok(new { message = "Authenticated" });
        }

    }
}
