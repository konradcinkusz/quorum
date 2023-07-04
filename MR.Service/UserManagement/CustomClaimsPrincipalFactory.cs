namespace MR.Service.UserManagement;

public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly IApplicationDbContext _dbContext;

    public CustomClaimsPrincipalFactory(IApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
                                                IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
        _dbContext = dbContext;
    }

    public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);

        bool isActiveSubscription = false;
        var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);
        if (subscription != null)
        {
            isActiveSubscription = subscription.IsActive();
        }

        // Add your claims here
        ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim("isActiveSubscription", isActiveSubscription.ToString())
                                                             });

        return principal;
    }
}