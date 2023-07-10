using Azure.Core;
using MR.Domain.Auth;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MR.Service.UserManagement;

public class MRUserManager : UserManager<ApplicationUser>
{
    private readonly IApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    public MRUserManager(IApplicationDbContext context, IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _context = context;
        _serviceProvider = services;
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
    {
        // Implement your custom logic here
        // Call the base implementation to persist the user to the data store
        var result = await base.CreateAsync(user);
        if (result.Succeeded)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await _context.Subscriptions.AddAsync(new Subscription { ApplicationUserId = user.Id }, cancellationToken);
            //sprawdz czy jest już dodany kwartał, jeżeli tak, to zainicjalizuj 
            var quarterList = _context.Quarters.GetCurrentAndFutureQuarters();
            foreach(var quarter in quarterList)
            {
                var signaturePool = new SignaturePool
                {
                    ApplicationUserId = user.Id,
                    QuarterId = quarter.Id,
                    Signatures = new List<Signature>()
                };

                for (int i = 0; i < quarter.PrimarySignatureCount; i++)
                {
                    signaturePool.Signatures.Add(new());
                }

                await _context.SignaturePools.AddAsync(signaturePool, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    public async Task<IEnumerable<Claim>> GetClaims(string userId)
    {
        var claimsFactory = _serviceProvider.GetRequiredService<CustomClaimsPrincipalFactory>();
        var user = await FindByIdAsync(userId);
        var principal = await claimsFactory.CreateAsync(user);
        return principal.Claims;
    }

    public async Task<bool> HasActiveSubscription(string applicationUserId)
    {
        var claimsFactory = _serviceProvider.GetRequiredService<CustomClaimsPrincipalFactory>();
        var user = await FindByIdAsync(applicationUserId);
        var principal = await claimsFactory.CreateAsync(user);
        var isActiveSubscriptionClaim = principal.Claims.FirstOrDefault(c => c.Type == "isActiveSubscription");

        if (isActiveSubscriptionClaim != null && bool.TryParse(isActiveSubscriptionClaim.Value, out bool isActiveSubscription))
        {
            return isActiveSubscription;
        }
        else
        {
            throw new ApplicationException("The isActiveSubscription claim is not present or cannot be parsed");
        }
    }
}
