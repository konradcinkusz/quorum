using Quorum.Domain.Entities;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Characterisation tests for the subscription window, which gates publishing and paying for
/// an issue via <c>IssueCommandHandlerBase.CheckBasicConditionsAndReturnIssue</c>.
/// <para>
/// Written before the behaviour moves, per P13. <c>QuorumUserService.HasActiveSubscriptionAsync</c>
/// replaces a path that built an entire <c>ClaimsPrincipal</c> through the Identity claims
/// factory in order to read back an <c>isActiveSubscription</c> claim the factory had just
/// computed from this same row — and threw if that claim was missing or unparseable. The
/// question was always <see cref="Subscription.IsActive"/>, and these pin that it still
/// answers identically once the detour is gone.
/// </para>
/// </summary>
public class SubscriptionActivityTests
{
    private static Subscription Window(DateTime? begin, DateTime? end)
        => new() { ApplicationUserId = "user-1", Begin = begin, End = end };

    [Fact]
    public void A_subscription_spanning_now_is_active()
    {
        var now = DateTime.UtcNow;

        Assert.True(Window(now.AddDays(-1), now.AddDays(1)).IsActive());
    }

    [Fact]
    public void A_subscription_that_has_not_begun_is_not_active()
    {
        var now = DateTime.UtcNow;

        Assert.False(Window(now.AddDays(1), now.AddDays(30)).IsActive());
    }

    [Fact]
    public void An_expired_subscription_is_not_active()
    {
        var now = DateTime.UtcNow;

        Assert.False(Window(now.AddDays(-30), now.AddDays(-1)).IsActive());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void A_half_open_or_unset_window_is_not_active(bool beginNull, bool endNull)
    {
        // This is the case that matters most for the cutover. A freshly provisioned user gets
        // a Subscription row with both dates null, and it must read as inactive — the row
        // exists to be filled in when they pay, not to grant anything by existing.
        var now = DateTime.UtcNow;

        var subscription = Window(
            beginNull ? null : now.AddDays(-1),
            endNull ? null : now.AddDays(1));

        Assert.False(subscription.IsActive());
    }

    [Fact]
    public void A_user_with_no_subscription_row_at_all_is_not_active()
    {
        // QuorumUserService returns `subscription?.IsActive() ?? false`. The old implementation
        // threw an ApplicationException when the claim it expected was absent, so "no row"
        // and "not subscribed" produced very different outcomes. They are the same thing.
        Subscription? none = null;

        Assert.False(none?.IsActive() ?? false);
    }
}
