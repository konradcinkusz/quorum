using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Quorum.Domain.Entities;
using Quorum.Persistence;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Cover for the residual risk the architecture review left open with a condition attached:
/// <i>"the <c>= false</c> initializer should become <c>= null</c> once there is a test to
/// prove what the publish flow expects."</i>
/// <para>
/// <c>EditIssueCommand.IsVerifyByAdmin</c> is nullable precisely so that "not supplied" can
/// mean "leave it alone" — the handler applies
/// <c>request.IsVerifyByAdmin ?? issue.IsVerifyByAdmin</c>. Defaulting it to <c>false</c>
/// made that unreachable from the user-facing route, which never sets the property, so every
/// user edit wrote <c>false</c>.
/// </para>
/// <para>
/// It is not cosmetic. <c>IsVerifyByAdmin</c> gates rating, publication and winner selection —
/// <c>CalculatePublishedIssueRatingForCurrentQuarter</c>,
/// <c>ChooseTheWinnerOfCurrentQuarter</c>, <c>GetCurrentQuarterPublishedIssues</c> and
/// <c>GetTheWinningIssuesForTheQuarterQuery</c> all filter on it. An edit therefore dropped an
/// initiative out of the quarter it was competing in, and told nobody.
/// </para>
/// </summary>
public sealed class IssueVerificationOnEditTests
{
    private const string Owner = "the-filer";
    private const string Administrator = "an-administrator";

    [Fact]
    public async Task A_user_edit_that_says_nothing_about_verification_leaves_it_alone()
    {
        await using var factory = new QuorumApplicationFactory(TestAuthentication.Configure);
        var issueId = await SeedVerifiedIssue(factory, Owner);
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1.0/Issue/edit-issue/{issueId}")
        {
            Content = JsonContent.Create(new
            {
                Title = "A revised title",
                Question = "Should the bridge be rebuilt?",
                Icon = "bridge",
                BackgroundColor = "#204060",
            }),
        };
        request.Headers.Add(TestAuthentication.UserIdHeader, Owner);
        using var response = await client.SendAsync(request);

        var stored = await Reload(factory, issueId);

        // The edit must have actually happened — otherwise this test would pass simply
        // because the request was rejected, which is exactly how it would rot.
        Assert.Equal("A revised title", stored.Title);
        Assert.True(stored.IsVerifyByAdmin, "a user's edit must not silently un-verify their own issue");
    }

    [Fact]
    public async Task An_administrator_can_still_clear_verification_explicitly()
    {
        // The other half of the change: null means "leave it alone", not "cannot be set".
        // The admin route always assigns the property, so it is unaffected by the default —
        // and this is what proves the fix did not simply make the field unwritable.
        await using var factory = new QuorumApplicationFactory(TestAuthentication.Configure);
        var issueId = await SeedVerifiedIssue(factory, Owner);
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(
            HttpMethod.Put, $"api/v1.0/AdminIssue/edit-issue-by-admin/{issueId}")
        {
            Content = JsonContent.Create(new
            {
                Title = "Unverified by an admin",
                Question = "Should the bridge be rebuilt?",
                Icon = "bridge",
                BackgroundColor = "#204060",
                IsVerifyByAdmin = false,
                ApplicationUserId = Owner,
                IssueVisibility = 0,
                IssueProcess = 0,
                RatingValue = 0,
            }),
        };
        request.Headers.Add(TestAuthentication.UserIdHeader, Administrator);
        request.Headers.Add(TestAuthentication.RoleHeader, "Admin");
        using var response = await client.SendAsync(request);

        var stored = await Reload(factory, issueId);

        Assert.Equal("Unverified by an admin", stored.Title);
        Assert.False(stored.IsVerifyByAdmin, "an explicit false from an admin must still clear it");
    }

    private static async Task<Guid> SeedVerifiedIssue(QuorumApplicationFactory factory, string userId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var issue = new Issue
        {
            Title = "A verified initiative",
            Question = "Should the bridge be rebuilt?",
            CreatedById = userId,
            CreatedByEmail = "filer@example.test",
            IsVerifyByAdmin = true,
        };

        context.Issues.Add(issue);
        await context.SaveChangesAsync();
        return issue.Id;
    }

    private static async Task<Issue> Reload(QuorumApplicationFactory factory, Guid id)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var issue = await context.Issues.FindAsync(id);
        Assert.NotNull(issue);
        return issue!;
    }
}
