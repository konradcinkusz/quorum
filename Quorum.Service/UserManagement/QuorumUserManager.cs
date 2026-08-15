using Quorum.Domain.Auth;

namespace Quorum.Service.UserManagement;

/// <summary>
/// ASP.NET Identity's user manager, extended only to run MR's per-user setup when a local
/// account is created.
/// <para>
/// <b>Transitional.</b> This class goes away with local identity (ADR 0001, step 6). The MR
/// domain logic it used to own — subscriptions, signature pools, the user roster, the
/// subscription check — now lives in <see cref="IQuorumUserService"/>, which has no dependency on
/// a local user table. All that remains here is the hook: while MR still creates accounts,
/// creating one provisions MR-side state, exactly as before. Once <c>authservice</c> owns
/// registration, the same provisioning runs on first sight of a user instead.
/// </para>
/// </summary>
public class QuorumUserManager : UserManager<ApplicationUser>
{
    private readonly IQuorumUserService _users;

    public QuorumUserManager(IQuorumUserService users, IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _users = users;
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
    {
        var result = await base.CreateAsync(user);

        if (result.Succeeded)
        {
            // Delegated rather than duplicated, so that the local-account path and the
            // first-sight path provision identically and cannot drift apart during the
            // cutover. The previous implementation also created its own
            // CancellationTokenSource and never cancelled it.
            await _users.EnsureProvisionedAsync(user.Id, user.Email, CancellationToken.None);
        }

        return result;
    }
}
