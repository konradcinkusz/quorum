namespace MR.Service.Features.Issues.PDF;

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
            var issue = await _context.Issues.FindAsync(request._issueId, cancellationToken);
            if (issue == null)
            {
                // Issue not found, handle accordingly (e.g., throw an exception, return false, etc.)
                return string.Empty;
            }

            // Convert the IFormFile to byte[] array
            byte[] pdfBytes;
            using (var stream = new MemoryStream())
            {
                await request._formFile.CopyToAsync(stream, cancellationToken);
                pdfBytes = stream.ToArray();
            }

            // Create an UploadedFile object to represent the PDF file
            UploadedFile uploadedFile = new UploadedFile(request._formFile.FileName, pdfBytes);

            // Upload the PDF to Cloudinary using the CloudinaryService
            var fileData = await _cloudinaryService.UploadPdfAsync(uploadedFile, cancellationToken);

            var cloudinaryFile = new CloudinaryFile()
            {
                PublicId = fileData.PublicId,
                SecureUri = fileData.SecureUri.AbsoluteUri,
                FileName = fileData.FileName,
            };

            _ = await _context.CloudinaryFileIssues.AddAsync(new CloudinaryFileIssue() { Issue = issue, CloudinaryFile = cloudinaryFile, CloudinaryFileIssueType = CloudinaryFileIssueType.UserSigned, ApplicationUserId = request._applicationUserId }, cancellationToken);

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return fileData.SecureUri.AbsoluteUri;
        }
    }
}