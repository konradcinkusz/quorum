namespace MR.Service.UserManagement;

/// <summary>
/// The MR-side facts about a user that are MR's own: whether they hold an active
/// subscription, what MR has to set up for them, and who MR has ever seen.
/// <para>
/// These used to live on <c>MRUserManager</c>, a subclass of ASP.NET Identity's
/// <c>UserManager</c>. That was the wrong home even before identity moved: subscriptions and
/// signature pools are MR's domain, not identity's, and hanging them off the user manager
/// meant they could only run where a local user table existed. Extracting them here is what
/// makes the cutover in ADR 0001 possible.
/// </para>
/// </summary>
public interface IMrUserService
{
    /// <summary>
    /// Records that a user has been seen, refreshes their cached email, and on first sight
    /// performs MR's per-user setup.
    /// </summary>
    Task EnsureProvisionedAsync(string userId, string? email, CancellationToken cancellationToken);

    /// <summary>Whether this user currently holds an active subscription.</summary>
    Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken cancellationToken);

    /// <summary>Every user MR knows about — the roster a new quarter issues signature pools to.</summary>
    Task<IReadOnlyList<string>> GetKnownUserIdsAsync(CancellationToken cancellationToken);

    /// <summary>The cached email for a user id, or <c>null</c> if MR has never seen them.</summary>
    Task<string?> GetEmailAsync(string userId, CancellationToken cancellationToken);
}

internal sealed class MrUserService : IMrUserService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MrUserService> _logger;

    public MrUserService(IApplicationDbContext context, ILogger<MrUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnsureProvisionedAsync(string userId, string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("A user id is required to provision MR-side state.", nameof(userId));
        }

        var now = DateTime.UtcNow;
        var known = await _context.MrUsers.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (known is not null)
        {
            known.LastSeenAt = now;
            if (!string.IsNullOrWhiteSpace(email) && known.Email != email)
            {
                known.Email = email;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        // First sight. Under the old model this ran inside MRUserManager.CreateAsync, because
        // MR was the thing creating users. It no longer is — authservice owns registration and
        // MR is never told about it — so provisioning happens the first time a user actually
        // turns up, which is the earliest moment MR can know they exist.
        _logger.LogInformation("Provisioning MR-side state for previously unseen user {UserId}", userId);

        await _context.MrUsers.AddAsync(
            new MrUser { Id = userId, Email = email, FirstSeenAt = now, LastSeenAt = now },
            cancellationToken);

        var hasSubscription = await _context.Subscriptions
            .AnyAsync(x => x.ApplicationUserId == userId, cancellationToken);

        if (!hasSubscription)
        {
            await _context.Subscriptions.AddAsync(
                new Subscription { ApplicationUserId = userId }, cancellationToken);
        }

        await AddSignaturePoolsForOpenQuartersAsync(userId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gives a user a signature pool in every quarter that has not yet passed, so someone who
    /// arrives mid-quarter can sign in it rather than waiting for the next one.
    /// </summary>
    private async Task AddSignaturePoolsForOpenQuartersAsync(string userId, CancellationToken cancellationToken)
    {
        var quarters = await _context.Quarters
            .GetCurrentAndFutureQuarters()
            .ToListAsync(cancellationToken);

        foreach (var quarter in quarters)
        {
            var alreadyHasPool = await _context.SignaturePools
                .AnyAsync(x => x.ApplicationUserId == userId && x.QuarterId == quarter.Id, cancellationToken);

            if (alreadyHasPool)
            {
                continue;
            }

            var pool = new SignaturePool
            {
                ApplicationUserId = userId,
                QuarterId = quarter.Id,
                Signatures = new List<Signature>(),
            };

            for (var i = 0; i < quarter.PrimarySignatureCount; i++)
            {
                pool.Signatures.Add(new Signature());
            }

            await _context.SignaturePools.AddAsync(pool, cancellationToken);
        }
    }

    public async Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken cancellationToken)
    {
        // The old implementation built a whole ClaimsPrincipal through the claims factory in
        // order to read back an "isActiveSubscription" claim the factory had just computed
        // from this very table — and threw if the claim was missing or unparseable. The
        // question was always a row lookup.
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(x => x.ApplicationUserId == userId, cancellationToken);

        return subscription?.IsActive() ?? false;
    }

    public async Task<IReadOnlyList<string>> GetKnownUserIdsAsync(CancellationToken cancellationToken)
        => await _context.MrUsers.Select(x => x.Id).ToListAsync(cancellationToken);

    public async Task<string?> GetEmailAsync(string userId, CancellationToken cancellationToken)
        => await _context.MrUsers
            .Where(x => x.Id == userId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync(cancellationToken);
}
