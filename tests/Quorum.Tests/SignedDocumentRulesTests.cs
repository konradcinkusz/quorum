using System.Text;
using Microsoft.AspNetCore.Http;
using Quorum.Service.Exceptions;
using Quorum.Service.FilesManagement;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Covers review finding F6 — the signed-document upload endpoint that accepted any
/// <see cref="IFormFile"/> with no checks on type, size or content.
/// </summary>
public class SignedDocumentRulesTests
{
    private static readonly byte[] PdfHeader = Encoding.ASCII.GetBytes("%PDF-1.7\n...");

    private static IFormFile File(long length, string contentType = "application/pdf")
        => new FormFile(new MemoryStream(new byte[Math.Min(length, 1024)]), 0, length, "file", "scan.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };

    [Fact]
    public void A_valid_pdf_upload_is_accepted()
    {
        SignedDocumentRules.ValidateOrThrow(File(2048));
        SignedDocumentRules.ValidateContentOrThrow(PdfHeader);
    }

    [Fact]
    public void An_empty_upload_is_rejected()
    {
        Assert.Throws<BadRequestException>(() => SignedDocumentRules.ValidateOrThrow(File(0)));
    }

    [Fact]
    public void An_oversized_upload_is_rejected_on_its_declared_length()
    {
        // Checked before the file is read, so a huge upload costs nothing to refuse. The
        // handler previously buffered the whole request into managed memory twice with no
        // ceiling at all.
        var oversized = File(SignedDocumentRules.MaxSizeBytes + 1);

        Assert.Throws<BadRequestException>(() => SignedDocumentRules.ValidateOrThrow(oversized));
    }

    [Fact]
    public void An_upload_exactly_at_the_limit_is_accepted()
    {
        SignedDocumentRules.ValidateOrThrow(File(SignedDocumentRules.MaxSizeBytes));
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("application/octet-stream")]
    [InlineData("text/html")]
    [InlineData("")]
    public void A_non_pdf_content_type_is_rejected(string contentType)
    {
        Assert.Throws<BadRequestException>(() => SignedDocumentRules.ValidateOrThrow(File(2048, contentType)));
    }

    [Fact]
    public void The_content_type_check_is_case_insensitive()
    {
        SignedDocumentRules.ValidateOrThrow(File(2048, "APPLICATION/PDF"));
    }

    [Fact]
    public void Content_that_is_not_a_pdf_is_rejected_however_it_is_labelled()
    {
        // The declared content type is chosen by the client. A PNG renamed to .pdf and sent
        // as application/pdf passes every metadata check and must still fail here.
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        Assert.Throws<BadRequestException>(() => SignedDocumentRules.ValidateContentOrThrow(png));
    }

    [Fact]
    public void Content_shorter_than_the_pdf_header_is_rejected()
    {
        Assert.Throws<BadRequestException>(() => SignedDocumentRules.ValidateContentOrThrow(new byte[] { 0x25, 0x50 }));
    }

    [Fact]
    public void The_stored_name_is_derived_from_the_issue_not_the_upload()
    {
        var issueId = Guid.NewGuid();

        var name = SignedDocumentRules.BuildStoredFileName(issueId);

        Assert.StartsWith($"signed-{issueId:N}-", name);
        Assert.EndsWith(".pdf", name);
    }

    [Fact]
    public void The_stored_name_is_unpredictable()
    {
        // While delivery stays public this suffix is effectively a share token, so it comes
        // from a CSPRNG. Uniqueness across many draws is a weak proxy for that, but it does
        // catch the regression that matters: someone swapping it back for a counter, a
        // timestamp, or the client's own file name.
        var issueId = Guid.NewGuid();

        var names = Enumerable.Range(0, 500)
            .Select(_ => SignedDocumentRules.BuildStoredFileName(issueId))
            .ToHashSet();

        Assert.Equal(500, names.Count);
    }

    [Fact]
    public void The_stored_name_is_url_safe()
    {
        // Base64 '+' and '/' would be re-encoded or path-split by the storage provider.
        var name = SignedDocumentRules.BuildStoredFileName(Guid.NewGuid());

        Assert.DoesNotContain('+', name);
        Assert.DoesNotContain('/', name);
        Assert.DoesNotContain('=', name);
    }
}
