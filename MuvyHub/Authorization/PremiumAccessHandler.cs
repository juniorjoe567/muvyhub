using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MuvyHub.Models;
using System;
using System.Threading.Tasks;

namespace MuvyHub.Authorization
{
    public class PremiumAccessHandler : AuthorizationHandler<PremiumAccessRequirement>
    {
        private readonly UserManager<AppUser> _userManager;

        public PremiumAccessHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PremiumAccessRequirement requirement)
        {
            var user = await _userManager.GetUserAsync(context.User);

            if (user == null)
            {
                context.Fail();
                return;
            }

            if (user.IsActive && user.ExpiryDate.HasValue && user.ExpiryDate.Value > DateTime.UtcNow)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
