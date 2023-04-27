using System.Collections.Generic;
using System.Security.Claims;

namespace MR.Service;

public class MRSignInManager : SignInManager<ApplicationUser>
{
    private readonly IApplicationDbContext _dbContext;

    public MRSignInManager(IApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<ApplicationUser>> logger, Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemes, IUserConfirmation<ApplicationUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _dbContext = dbContext;
    }

    public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

        if (result.Succeeded)
        {
            bool isActiveSubscription = false;
            var user = await UserManager.FindByNameAsync(userName);
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);
            if(subscription != null)
            {
                isActiveSubscription = subscription.IsActive();
            }

            //var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            ((ClaimsIdentity)Context.User.Identity).AddClaim(new Claim("isActiveSubscription", isActiveSubscription.ToString()));
        }

        return result;
    }
}
