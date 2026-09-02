using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Quorum.Domain.Entities;
using Quorum.Domain.Enum;
using Quorum.Domain.Enums;
using Quorum.Persistence;
using Quorum.Service.FilesManagement;
using Quorum.Service.FilesManagement.Models;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Regression cover for the open half of review finding F6 — <i>signed petition documents are
/// delivered publicly</i>.
/// <para>
/// The asset is a wet-signature sheet: real names, real signatures, tied to a named political
/// position. Before this, the upload call handed its caller a permanent, unauthenticated CDN
/// link to exactly that, and the document was stored world-readable besides. The fix has two
/// halves and only one of them can be tested from here.
/// </para>
/// <para>
/// <b>What these tests prove:</b> the endpoint that hands out a download URL applies the same
/// eligibility predicate the upload path uses, and refuses an ineligible caller identically to
/// one asking about an issue that does not exist.
/// </para>
/// <para>
/// <b>What they cannot prove:</b> that Cloudinary actually refuses an unsigned request for an
/// asset stored with the <c>authenticated</c> delivery type. That needs real credentials and a
/// round trip to a third party. <see cref="RecordingCloudinaryService"/> stands in for the
/// storage provider, so what is asserted below is our authorization decision and nothing about
/// theirs. The delivery type itself is asserted by
/// <see cref="SignedDocumentStorageTests"/>, which is also not a round trip.
/// </para>
/// </summary>
public sealed class SignedDocumentDeliveryTests
{
    private const string Signatory = "the-person-who-signed-it";
    private const string Stranger = "somebody-else-entirely";
    private const string Route = "api/v1.0/Issue";

    [Fact]
    public async Task The_signatory_is_given_a_url_for_their_own_document()
    {
        var cloudinary = new RecordingCloudinaryService();
        await using var factory = Factory(cloudinary);
        var issueId = await SeedSignedDocument(factory, Signatory);

        var response = await RequestUrl(factory, issueId, actingAs: Signatory);

        Assert.True(response!.Success, "the person who signed the sheet should be able to fetch it");
        Assert.Equal(RecordingCloudinaryService.Url, response.Data);
    }

    [Fact]
    public async Task A_stranger_is_refused_and_cannot_tell_the_document_exists()
    {
        var cloudinary = new RecordingCloudinaryService();
        await using var factory = Factory(cloudinary);
        var realIssue = await SeedSignedDocument(factory, Signatory);
        var neverExisted = Guid.NewGuid();

        var somebodyElses = await RequestUrl(factory, realIssue, actingAs: Stranger);
        var noSuchIssue = await RequestUrl(factory, neverExisted, actingAs: Stranger);

        Assert.False(somebodyElses!.Success);
        Assert.False(noSuchIssue!.Success);
        Assert.Equal(noSuchIssue.StatusCode, somebodyElses.StatusCode);

        // The load-bearing one. A refusal that still built a URL would have handed the
        // storage provider a public id for a document the caller may not see, and any
        // logging or caching on that path would then hold it.
        Assert.Equal(0, cloudinary.UrlsBuilt);
    }

    [Fact]
    public async Task An_anonymous_caller_is_challenged_rather_than_served()
    {
        var cloudinary = new RecordingCloudinaryService();
        await using var factory = Factory(cloudinary);
        var issueId = await SeedSignedDocument(factory, Signatory);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"{Route}/signed-document-url/{issueId}");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, cloudinary.UrlsBuilt);
    }

    [Fact]
    public async Task The_url_is_asked_for_with_a_short_lifetime()
    {
        var cloudinary = new RecordingCloudinaryService();
        await using var factory = Factory(cloudinary);
        var issueId = await SeedSignedDocument(factory, Signatory);

        _ = await RequestUrl(factory, issueId, actingAs: Signatory);

        // A bound rather than the exact value: the constant is a judgement that may be
        // revised, and pinning it here would make this test a copy of it. What must not
        // change quietly is the order of magnitude — an hour would defeat the point, since
        // the URL is the credential once it has been handed out.
        Assert.True(
            cloudinary.LastLifetime > TimeSpan.Zero && cloudinary.LastLifetime <= TimeSpan.FromMinutes(15),
            $"a download URL should be short-lived; asked for {cloudinary.LastLifetime}");
    }

    // --- plumbing ------------------------------------------------------------------------

    private static QuorumApplicationFactory Factory(ICloudinaryService cloudinary)
        => new(services =>
        {
            TestAuthentication.Configure(services);
            services.AddScoped(_ => cloudinary);
        });

    /// <summary>
    /// Seeds an issue that satisfies every clause of the eligibility rule, with a signed
    /// document attached by <paramref name="userId"/>. Ids are fresh per test because the
    /// application's InMemory provider uses one fixed database name, so the store is shared
    /// across factories within a run.
    /// </summary>
    private static async Task<Guid> SeedSignedDocument(QuorumApplicationFactory factory, string userId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var quarter = new Quarter { Year = 2026, QuarterNumber = 3 };
        var pool = new SignaturePool { ApplicationUserId = userId, Quarter = quarter };

        var issue = new Issue
        {
            Title = "A question for the electorate",
            Question = "Should the bridge be rebuilt?",
            CreatedById = "whoever-filed-it",
            CreatedByEmail = "filer@example.test",
            IssueProcess = IssueProcess.EndedInCurrentQuarter,
            IssueVisibility = IssueVisibility.VisibleForAll,
            Signatures = new List<Signature> { new() { SignaturePool = pool } },
        };

        context.Issues.Add(issue);
        await context.SaveChangesAsync();

        context.CloudinaryFileIssues.Add(new CloudinaryFileIssue
        {
            Issue = issue,
            ApplicationUserId = userId,
            CloudinaryFileIssueType = CloudinaryFileIssueType.UserSigned,
            CloudinaryFile = new CloudinaryFile
            {
                PublicId = "signed/abc123",
                // Recorded as it comes back from the provider. It is no longer handed to
                // anyone: reaching the document goes through the endpoint under test.
                SecureUri = "https://res.cloudinary.test/authenticated/signed/abc123.pdf",
                FileName = "signed-sheet.pdf",
            },
        });
        await context.SaveChangesAsync();

        return issue.Id;
    }

    private static async Task<ApiResponseEnvelope?> RequestUrl(
        QuorumApplicationFactory factory, Guid issueId, string actingAs)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/signed-document-url/{issueId}");
        request.Headers.Add(TestAuthentication.UserIdHeader, actingAs);

        using var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<ApiResponseEnvelope>();
    }

    private sealed class ApiResponseEnvelope
    {
        public bool Success { get; set; }
        public string? Data { get; set; }
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// Stands in for the storage provider and records what it was asked for. It counts calls
    /// rather than merely answering them, because "was a URL built at all" is the assertion
    /// that catches a refusal which leaks on the way out.
    /// </summary>
    private sealed class RecordingCloudinaryService : ICloudinaryService
    {
        public const string Url = "https://res.cloudinary.test/signed-and-expiring";

        public int UrlsBuilt { get; private set; }
        public TimeSpan LastLifetime { get; private set; }

        public string BuildSignedDownloadUrl(string publicId, TimeSpan lifetime)
        {
            UrlsBuilt++;
            LastLifetime = lifetime;
            return Url;
        }

        public Task<ImageData> UploadImageAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<FileData> UploadPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<FileData> UploadSignedPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
