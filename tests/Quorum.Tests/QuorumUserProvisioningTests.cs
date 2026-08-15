using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Quorum.Domain.Entities;
using Quorum.Persistence;
using Quorum.Service.UserManagement;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Characterisation of first-sight provisioning (ADR 0001). Registration happens in
/// authservice and Quorum is never told, so the first authenticated appearance — the BFF
/// session — is the moment Quorum sets a user up: the projection row, the subscription
/// row, and a signature pool for every open quarter. These pin that behaviour through
/// <see cref="IQuorumUserService"/>, the seam the session service calls.
/// </summary>
public class QuorumUserProvisioningTests
{
    private static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static QuorumUserService NewService(ApplicationDbContext context)
        => new(context, NullLogger<QuorumUserService>.Instance);

    private static Quarter OpenQuarter(int signatureCount = 3)
    {
        var now = DateTime.UtcNow;
        return new Quarter
        {
            Id = Guid.NewGuid(),
            Year = now.Year,
            QuarterNumber = (now.Month - 1) / 3 + 1,
            PrimarySignatureCount = signatureCount,
        };
    }

    [Fact]
    public async Task First_sight_creates_the_projection_the_subscription_and_pools_for_open_quarters()
    {
        using var context = NewContext();
        context.Quarters.Add(OpenQuarter(signatureCount: 3));
        await context.SaveChangesAsync(CancellationToken.None);

        await NewService(context).EnsureProvisionedAsync("user-1", "user-1@example.test", CancellationToken.None);

        var known = Assert.Single(context.QuorumUsers);
        Assert.Equal("user-1", known.Id);
        Assert.Equal("user-1@example.test", known.Email);

        var subscription = Assert.Single(context.Subscriptions);
        Assert.Equal("user-1", subscription.ApplicationUserId);

        var pool = Assert.Single(context.SignaturePools);
        Assert.Equal("user-1", pool.ApplicationUserId);
        Assert.Equal(3, context.Signatures.Count());
    }

    [Fact]
    public async Task A_second_sighting_updates_last_seen_and_creates_nothing_new()
    {
        using var context = NewContext();
        context.Quarters.Add(OpenQuarter());
        await context.SaveChangesAsync(CancellationToken.None);

        var service = NewService(context);
        await service.EnsureProvisionedAsync("user-1", "user-1@example.test", CancellationToken.None);
        var firstSeen = context.QuorumUsers.Single().FirstSeenAt;

        await service.EnsureProvisionedAsync("user-1", "user-1@example.test", CancellationToken.None);

        Assert.Single(context.QuorumUsers);
        Assert.Single(context.Subscriptions);
        Assert.Single(context.SignaturePools);
        Assert.Equal(firstSeen, context.QuorumUsers.Single().FirstSeenAt);
    }

    [Fact]
    public async Task A_changed_email_refreshes_the_cached_projection()
    {
        // The projection is a display cache, not an authority: when the identity service
        // reports a different email at the next session, the cache follows it.
        using var context = NewContext();
        var service = NewService(context);
        await service.EnsureProvisionedAsync("user-1", "old@example.test", CancellationToken.None);

        await service.EnsureProvisionedAsync("user-1", "new@example.test", CancellationToken.None);

        Assert.Equal("new@example.test", context.QuorumUsers.Single().Email);
    }

    [Fact]
    public async Task Provisioning_without_a_user_id_is_refused()
    {
        using var context = NewContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            NewService(context).EnsureProvisionedAsync(" ", "a@example.test", CancellationToken.None));
    }

    [Fact]
    public async Task The_batch_email_lookup_returns_only_known_users()
    {
        using var context = NewContext();
        var service = NewService(context);
        await service.EnsureProvisionedAsync("user-1", "one@example.test", CancellationToken.None);
        await service.EnsureProvisionedAsync("user-2", "two@example.test", CancellationToken.None);

        var emails = await service.GetEmailsAsync(new[] { "user-1", "user-2", "never-seen" }, CancellationToken.None);

        Assert.Equal(2, emails.Count);
        Assert.Equal("one@example.test", emails["user-1"]);
        Assert.False(emails.ContainsKey("never-seen"));
    }
}
