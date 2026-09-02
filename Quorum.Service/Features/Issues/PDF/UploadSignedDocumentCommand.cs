namespace Quorum.Service.Features.Issues.PDF;

public class UploadSignedDocumentCommand : IRequest<string>
{
    readonly Guid _issueId;
    readonly IFormFile _formFile;
    readonly string _applicationUserId;

    public UploadSignedDocumentCommand(Guid issueId, IFormFile formFile, string applicationUserId)
    {
        _issueId = issueId;
        _formFile = formFile;
        _applicationUserId = applicationUserId;
    }

    internal class UploadSignedDocumentCommandHandler : CommandHandlerBase<UploadSignedDocumentCommand, string>
    {
        readonly ICloudinaryService _cloudinaryService;

        public UploadSignedDocumentCommandHandler(IApplicationDbContext context, ILogger<UploadSignedDocumentCommand> logger, ICloudinaryService cloudinaryService) : base(context, logger)
        {
            _cloudinaryService = cloudinaryService;
        }

        public override async Task<string> Handle(UploadSignedDocumentCommand request, CancellationToken cancellationToken)
        {
            // Cheap checks first: refuse an oversized or non-PDF upload on its declared
            // metadata before any of it is read into memory.
            SignedDocumentRules.ValidateOrThrow(request._formFile);

            // Who may attach a signed document is not "who owns the issue" — it is "who
            // signed it", and only once the issue has ended as a winner in the current
            // quarter. RestrictToSignatory is that rule, and the winners list and the
            // download endpoint call the same one, so no two of the three can disagree about
            // eligibility. Previously there was no check at all: any authenticated caller
            // could attach a document to any issue by presenting its id.
            var issue = await _context.Issues
                .Where(x => x.Id == request._issueId)
                .RestrictToSignatory(request._applicationUserId)
                .FirstOrDefaultAsync(cancellationToken);

            // Same result whether the issue does not exist, has not ended, or was not signed
            // by this user, so the endpoint cannot be used to probe for issue ids.
            if (issue == null)
            {
                throw new NotFoundException(nameof(Issue), request._issueId);
            }

            byte[] pdfBytes;
            using (var stream = new MemoryStream())
            {
                await request._formFile.CopyToAsync(stream, cancellationToken);
                pdfBytes = stream.ToArray();
            }

            // The declared content type is chosen by the client; the bytes are the evidence.
            SignedDocumentRules.ValidateContentOrThrow(pdfBytes);

            // Stored under a name this application generates. The client-supplied file name
            // used to be passed straight through to the storage provider.
            var storedFileName = SignedDocumentRules.BuildStoredFileName(request._issueId);
            var uploadedFile = new UploadedFile(storedFileName, pdfBytes);

            // Not UploadPdfAsync. That one stores the blank sheet an administrator generates
            // for people to print, which is public by design; this document comes back with
            // real names and signatures on it and is stored so that its URL is worthless
            // without a signature.
            var fileData = await _cloudinaryService.UploadSignedPdfAsync(uploadedFile, cancellationToken);

            var cloudinaryFile = new CloudinaryFile()
            {
                PublicId = fileData.PublicId,
                SecureUri = fileData.SecureUri.AbsoluteUri,
                FileName = fileData.FileName,
            };

            _ = await _context.CloudinaryFileIssues.AddAsync(new CloudinaryFileIssue() { Issue = issue, CloudinaryFile = cloudinaryFile, CloudinaryFileIssueType = CloudinaryFileIssueType.UserSigned, ApplicationUserId = request._applicationUserId }, cancellationToken);

            _ = await _context.SaveChangesAsync(cancellationToken);

            // The stored name, not the URL. This method used to return
            // fileData.SecureUri.AbsoluteUri, which wrote a permanent, unauthenticated link
            // to a page of real signatures into an HTTP response body — and from there into
            // whatever history, proxy log and referrer header saw it. A caller that wants the
            // document asks GetSignedDocumentDownloadUrlQuery for a short-lived one.
            //
            // Nothing consumed the old value: the client declares this call as
            // ApiResponse<bool> and discards the payload.
            return storedFileName;
        }
    }
}
