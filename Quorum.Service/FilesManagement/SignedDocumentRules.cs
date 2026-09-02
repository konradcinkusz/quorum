using System.Security.Cryptography;

namespace Quorum.Service.FilesManagement;

/// <summary>
/// What a signed-petition upload has to be before it is sent anywhere. Applied at the one
/// place uploads enter the system, so the rules cannot be bypassed by a second endpoint
/// later forgetting them.
/// </summary>
public static class SignedDocumentRules
{
    /// <summary>
    /// Upper bound on an accepted upload. A scanned signature sheet is a few hundred KB;
    /// 10 MB is generous for a multi-page scan and still small enough that a handful of
    /// concurrent uploads cannot exhaust the process. Without a cap the handler buffered
    /// the whole request into managed memory twice, so the ceiling was the machine's.
    /// </summary>
    public const long MaxSizeBytes = 10 * 1024 * 1024;

    public const string RequiredContentType = "application/pdf";

    /// <summary>
    /// How long a signed download URL stays valid.
    /// <para>
    /// Once handed out, the URL <i>is</i> the credential — anyone holding it can fetch the
    /// document — so the useful question is not "is it secret" but "for how long is a copy of
    /// it worth anything". Five minutes is long enough for a browser to follow a link the
    /// user just clicked and short enough that the same string in a proxy log tomorrow is
    /// inert. It is not long enough to email to somebody, which is the point.
    /// </para>
    /// </summary>
    public static readonly TimeSpan DownloadUrlLifetime = TimeSpan.FromMinutes(5);

    /// <summary>`%PDF-` — the header every PDF starts with.</summary>
    private static readonly byte[] PdfMagicBytes = { 0x25, 0x50, 0x44, 0x46, 0x2D };

    /// <summary>
    /// Rejects anything that is not a plausibly-real PDF within the size cap.
    /// </summary>
    /// <exception cref="BadRequestException">
    /// Thrown with a message safe to show the user. Checks run cheapest-first so an
    /// oversized upload is refused on its declared length, before it is read into memory.
    /// </exception>
    public static void ValidateOrThrow(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("No file was uploaded.");
        }

        if (file.Length > MaxSizeBytes)
        {
            throw new BadRequestException(
                $"The file is larger than the {MaxSizeBytes / (1024 * 1024)} MB limit.");
        }

        if (!string.Equals(file.ContentType, RequiredContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Only PDF files can be uploaded.");
        }
    }

    /// <summary>
    /// Confirms the bytes are a PDF, not merely labelled as one. The declared content type
    /// is chosen by the client and is not evidence of anything on its own.
    /// </summary>
    public static void ValidateContentOrThrow(byte[] content)
    {
        if (content.Length < PdfMagicBytes.Length)
        {
            throw new BadRequestException("The uploaded file is not a valid PDF.");
        }

        for (var i = 0; i < PdfMagicBytes.Length; i++)
        {
            if (content[i] != PdfMagicBytes[i])
            {
                throw new BadRequestException("The uploaded file is not a valid PDF.");
            }
        }
    }

    /// <summary>
    /// The name the document is stored under. Generated here rather than taken from the
    /// upload: a client-supplied file name is attacker-controlled text that would otherwise
    /// reach the storage provider's public id, and it is not needed for anything — the
    /// document is identified by its issue and uploader, both of which are recorded in the
    /// database row.
    /// <para>
    /// The suffix is 256 bits from a CSPRNG rather than a <c>Guid</c>. That was originally
    /// mitigation for public delivery — the name was the only thing standing between a
    /// stranger and a page of real signatures, which made it a share token, and a GUID is
    /// not a secret. Delivery is authenticated now (F6 closed by #19), so the name is no
    /// longer load-bearing and it stays unpredictable anyway: defence in depth costs nothing
    /// here, and a predictable name would become load-bearing again the moment somebody
    /// added a second storage path.
    /// </para>
    /// </summary>
    public static string BuildStoredFileName(Guid issueId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return $"signed-{issueId:N}-{token}.pdf";
    }
}
