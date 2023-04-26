namespace MR.Service;

public class MRUserManager : UserManager<ApplicationUser>
{
    private readonly IApplicationDbContext _context;
    public MRUserManager(IApplicationDbContext context, IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _context = context;
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
    {
        // Implement your custom logic here
        // Call the base implementation to persist the user to the data store
        var result = await base.CreateAsync(user);
        if (result.Succeeded)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            await _context.Subscriptions.AddAsync(new Subscription { ApplicationUserId = user.Id }, token);
            await _context.SaveChangesAsync(token);
        }
        return result;
    }
}
