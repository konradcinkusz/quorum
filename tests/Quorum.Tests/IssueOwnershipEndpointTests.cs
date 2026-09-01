using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quorum.Domain.Entities;
using Quorum.Persistence;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Regression cover for F2 — <i>no ownership check on any issue mutation</i>, the review's most
/// serious correctness finding.
/// <para>
/// The fix was a good one: <c>IssueOwnerScope</c> has no public constructor and two factories,
/// so the compiler rejects a call site that has not chosen a scope. But the compiler only
/// enforces that a choice was <b>made</b> — <c>Administrator()</c> compiles exactly as well as
/// <c>OwnedBy(userId)</c>. Until now the only thing standing between a future edit and a
/// re-introduced IDOR was that somebody noticed during review, which is precisely what failed
/// the first time, for three years.
/// </para>
/// <para>
/// The property being protected is not merely "a stranger is refused". It is that <b>a stranger
/// and a non-existent issue are refused identically</b>, so the endpoints cannot be used to
/// probe for other users' issue ids. A test that accepted one shape of refusal for the first
/// and another for the second would lock in the information leak the fix removed.
/// </para>
/// </summary>
public sealed class IssueOwnershipEndpointTests
{
    private const string Owner = "owner-of-the-issue";
    private const string Stranger = "somebody-else-entirely";

    [Fact]
    public async Task An_owner_can_read_their_own_issue_for_edit()
    {
        await using var factory = new QuorumApplicationFactory(TestAuthentication.Configure);
        var issueId = await SeedIssueOwnedBy(factory, Owner);

        var response = await ReadForEdit(factory, issueId, actingAs: Owner);

        Assert.True(response!.Success, "the owner should be able to read their own issue");
    }

    [Fact]
    public async Task A_stranger_is_refused_and_cannot_tell_the_issue_exists()
    {
        await using var factory = new QuorumApplicationFactory(TestAuthentication.Configure);
        var realIssue = await SeedIssueOwnedBy(factory, Owner);
        var neverExisted = Guid.NewGuid();

        var somebodyElsesIssue = await ReadForEdit(factory, realIssue, actingAs: Stranger);
        var noSuchIssue = await ReadForEdit(factory, neverExisted, actingAs: Stranger);

        Assert.False(somebodyElsesIssue!.Success);
        Assert.False(noSuchIssue!.Success);

        // The load-bearing assertion. If these ever diverge, the endpoint has become an
        // oracle for "does this issue id exist", which is what F3 was and what F2's fix
        // deliberately closed.
        Assert.Equal(noSuchIssue.StatusCode, somebodyElsesIssue.StatusCode);
        Assert.Equal(noSuchIssue.Errors, somebodyElsesIssue.Errors);
    }

    [Fact]
    public async Task A_stranger_cannot_edit_or_archive_somebody_elses_issue()
    {
        await using var factory = new QuorumApplicationFactory(TestAuthentication.Configure);
        var issueId = await SeedIssueOwnedBy(factory, Owner);
        using var client = factory.CreateClient();

        using var edit = await Send(client, HttpMethod.Put, $"{Route}/edit-issue/{issueId}", Stranger,
            JsonContent.Create(new { Title = "Renamed by a stranger", Question = "?" }));
        using var archive = await Send(client, HttpMethod.Delete, $"{Route}/archive-issue/{issueId}", Stranger);

        Assert.False((await Envelope(edit))!.Success);
        Assert.False((await Envelope(archive))!.Success);

        // And the issue is untouched — a refusal that still wrote would be worse than no
        // refusal at all, because it would look safe.
        using var scope = factory.Services.CreateScope();
        var stored = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Issues.FindAsync(issueId);
        Assert.Equal("A question for the electorate", stored!.Title);
    }

    [Fact]
    public async Task An_anonymous_caller_is_challenged_rather_than_served()
    {
        await using var factory = new QuorumApplicationFactory(TestAuthentication.Configure);
        var issueId = await SeedIssueOwnedBy(factory, Owner);
        using var client = factory.CreateClient();

        // No identity header at all: the handler returns NoResult and [Authorize] challenges.
        using var response = await client.GetAsync($"{Route}/get-issue-by-id-for-edit?id={issueId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- plumbing ------------------------------------------------------------------------

    private const string Route = "api/v1.0/Issue";

    /// <summary>
    /// Each test seeds its own issue id and reads it back by that id. The application's
    /// InMemory provider uses one fixed database name, so the store is shared across
    /// factories in a run — unique ids per test is what keeps them from seeing each other,
    /// rather than reconfiguring the provider the application actually ships.
    /// </summary>
    private static async Task<Guid> SeedIssueOwnedBy(QuorumApplicationFactory factory, string userId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var issue = new Issue
        {
            Title = "A question for the electorate",
            Question = "Should the bridge be rebuilt?",
            CreatedById = userId,
            CreatedByEmail = "filer@example.test",
        };

        context.Issues.Add(issue);
        await context.SaveChangesAsync();

        // Read the key back rather than assigning one: Id is [DatabaseGenerated(Identity)],
        // and letting the provider assign it is what the application does.
        return issue.Id;
    }

    private static async Task<ApiResponseEnvelope?> ReadForEdit(
        QuorumApplicationFactory factory, Guid id, string actingAs)
    {
        using var client = factory.CreateClient();
        using var response = await Send(client, HttpMethod.Get, $"{Route}/get-issue-by-id-for-edit?id={id}", actingAs);
        return await Envelope(response);
    }

    private static Task<HttpResponseMessage> Send(
        HttpClient client, HttpMethod method, string url, string userId, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Add(TestAuthentication.UserIdHeader, userId);
        return client.SendAsync(request);
    }

    private static async Task<ApiResponseEnvelope?> Envelope(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<ApiResponseEnvelope>();

    /// <summary>
    /// A local shape for <c>ApiResponse&lt;T&gt;</c>: these tests care about the envelope, not
    /// the payload, and binding to the generic type would drag the DTO in for no benefit.
    /// </summary>
    private sealed class ApiResponseEnvelope
    {
        public bool Success { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }
    }
}

/// <summary>
/// Substitutes the authentication scheme so these tests exercise <b>authorization</b>.
/// <para>
/// Deliberately not a real token: the server validates RS256 bearer tokens against an external
/// authservice instance's JWKS, and standing one up here would be testing that instance. The
/// identity arrives in a header instead, so a test can act as any user without the run
/// depending on anything outside the process.
/// </para>
/// </summary>
internal static class TestAuthentication
{
    public const string Scheme = "Test";
    public const string UserIdHeader = "X-Test-User";

    public static void Configure(IServiceCollection services)
        => services
            .AddAuthentication(Scheme)
            .AddScheme<AuthenticationSchemeOptions, Handler>(Scheme, _ => { });

    private sealed class Handler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public Handler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserIdHeader, out var userId)
                || string.IsNullOrWhiteSpace(userId))
            {
                // NoResult rather than Fail, so [Authorize] challenges with 401 exactly as it
                // would for a request carrying no token.
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // TestAuthentication.Scheme, qualified: inside an AuthenticationHandler, a bare
            // `Scheme` binds to the inherited AuthenticationScheme property, not to the const
            // on the enclosing class.
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId!) },
                TestAuthentication.Scheme);

            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(identity), TestAuthentication.Scheme)));
        }
    }
}
