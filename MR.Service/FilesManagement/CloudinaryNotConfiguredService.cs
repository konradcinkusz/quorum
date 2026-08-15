namespace MR.Service.FilesManagement;

/// <summary>
/// Stands in for <see cref="CloudinaryService"/> when no Cloudinary credentials are
/// configured, so the application starts and every feature that does not touch file storage
/// keeps working (P8 — optional dependencies degrade, they do not fail startup).
/// <para>
/// It throws rather than silently succeeding, and that is deliberate. The usual no-op
/// fallback is right for telemetry or email; it is wrong here. The files travelling through
/// this service are wet-signature petition documents, and a stub that accepted an upload and
/// discarded it would report success to a user whose signature sheet had just been thrown
/// away. Failing loudly on the upload path is the honest degradation for this domain.
/// </para>
/// </summary>
internal sealed class CloudinaryNotConfiguredService : ICloudinaryService
{
    private const string Explanation =
        "File storage is not configured, so this operation cannot be completed. Set " +
        "CloudinaryOpt:Cloud, CloudinaryOpt:ApiKey and CloudinaryOpt:ApiSecret — via " +
        "'dotnet user-secrets' in development, or CloudinaryOpt__Cloud / __ApiKey / " +
        "__ApiSecret in a deployed environment. They are deliberately absent from " +
        "appsettings.json.";

    public Task<ImageData> UploadImageAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
        => throw new InvalidOperationException(Explanation);

    public Task<FileData> UploadPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
        => throw new InvalidOperationException(Explanation);
}
