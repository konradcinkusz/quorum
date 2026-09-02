using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.DependencyInjection;
using Quorum.Domain.Entities;
using Quorum.Domain.Enums;
using Quorum.Persistence;
using Quorum.Service.FilesManagement;
using Quorum.Service.FilesManagement.Models;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// The upload half of review finding F6's open row: a signed petition sheet must not be stored
/// world-readable, and its public URL must not be handed back to the caller.
/// <para>
/// Both were true before. <c>UploadSignedDocumentCommand</c> called the same
/// <c>UploadPdfAsync</c> that stores the <i>blank</i> sheet — public by design and correct for
/// that document — and then returned <c>fileData.SecureUri.AbsoluteUri</c>, writing a
/// permanent unauthenticated link to a page of real signatures into an HTTP response body.
/// </para>
/// <para>
/// What is asserted here is the choice of storage method and the shape of the answer. Whether
/// Cloudinary honours the <c>authenticated</c> delivery type is theirs to enforce and needs
/// real credentials to observe; this cannot and does not claim it.
/// </para>
/// </summary>
public sealed class SignedDocumentStorageTests
{
    private const string Signatory = "the-person-who-signed-it";
    private const string Route = "api/v1.0/Issue";

    /// <summary>What the storage provider would hand back. Nobody should ever see it.</summary>
    private const string PublicUrl = "https://res.cloudinary.test/upload/signed-sheet.pdf";

    [Fact]
    public async Task A_signed_sheet_is_stored_through_the_non_public_path()
    {
        var cloudinary = new RecordingCloudinaryService();
        await using var factory = Factory(cloudinary);
        var issueId = await SeedEligibleIssue(factory, Signatory);

        var response = await Upload(factory, issueId, actingAs: Signatory);

        Assert.True(
            response!.Success,
            $"the upload should have been accepted; got {string.Join("; ", response.Errors ?? new List<string>())}");

        // The whole assertion. UploadPdfAsync is the right method for the blank sheet and the
        // wrong one for this document, and the two differ by one word at the call site.
        Assert.Equal(1, cloudinary.SignedUploads);
        Assert.Equal(0, cloudinary.PublicUploads);
    }

    [Fact]
    public async Task The_response_does_not_carry_the_documents_public_url()
    {
        var cloudinary = new RecordingCloudinaryService();
        await using var factory = Factory(cloudinary);
        var issueId = await SeedEligibleIssue(factory, Signatory);

        var response = await Upload(factory, issueId, actingAs: Signatory);

        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
        Assert.DoesNotContain(PublicUrl, response.Data);

        // Not merely "not that exact string": nothing URL-shaped belongs in this answer at
        // all. A future edit that returned a *different* provider URL would be the same
        // defect wearing a different host name.
        Assert.DoesNotContain("http", response.Data, StringComparison.OrdinalIgnoreCase);
    }

    // --- plumbing ------------------------------------------------------------------------

    private static QuorumApplicationFactory Factory(ICloudinaryService cloudinary)
        => new(services =>
        {
            TestAuthentication.Configure(services);
            services.AddScoped(_ => cloudinary);
        });

    private static async Task<Guid> SeedEligibleIssue(QuorumApplicationFactory factory, string userId)
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

        return issue.Id;
    }

    private static async Task<ApiResponseEnvelope?> Upload(
        QuorumApplicationFactory factory, Guid issueId, string actingAs)
    {
        using var client = factory.CreateClient();

        // Has to survive both of SignedDocumentRules' checks: the declared content type, and
        // the %PDF- magic bytes, which are the evidence the declared type is not.
        var pdf = new ByteArrayContent(Encoding.ASCII.GetBytes("%PDF-1.7\n% a signed sheet\n"));
        pdf.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        using var form = new MultipartFormDataContent { { pdf, "file", "scan.pdf" } };
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{Route}/upload-signed-document/{issueId}")
        {
            Content = form,
        };
        request.Headers.Add(TestAuthentication.UserIdHeader, actingAs);

        using var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<ApiResponseEnvelope>();
    }

    private sealed class ApiResponseEnvelope
    {
        public bool Success { get; set; }
        public string? Data { get; set; }
        public List<string>? Errors { get; set; }
    }

    /// <summary>
    /// Counts which storage path was taken. Both upload methods answer, so a wrong choice
    /// shows up as a count rather than as an exception — an exception would also have failed
    /// the test, but for a reason that reads like plumbing rather than like the finding.
    /// </summary>
    private sealed class RecordingCloudinaryService : ICloudinaryService
    {
        public int PublicUploads { get; private set; }
        public int SignedUploads { get; private set; }

        public Task<FileData> UploadPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
        {
            PublicUploads++;
            return Task.FromResult(Stored(uploadedFile));
        }

        public Task<FileData> UploadSignedPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
        {
            SignedUploads++;
            return Task.FromResult(Stored(uploadedFile));
        }

        private static FileData Stored(UploadedFile uploadedFile)
            => new(
                new RawUploadResult { PublicId = "signed/abc123", SecureUri = new Uri(PublicUrl) },
                uploadedFile.Name);

        public Task<ImageData> UploadImageAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public string BuildSignedDownloadUrl(string publicId, TimeSpan lifetime)
            => throw new NotSupportedException();
    }
}
