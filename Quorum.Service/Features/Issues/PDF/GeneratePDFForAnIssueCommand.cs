namespace Quorum.Service.Features.Issues.PDF;

public class GeneratePDFForAnIssueCommand : IRequest<string>
{
    readonly Guid _issueId;
    public GeneratePDFForAnIssueCommand(Guid issueId)
    {
        _issueId = issueId;
    }

    internal sealed class GeneratePDFForAnIssueCommandHandler : CommandHandlerBase<GeneratePDFForAnIssueCommand, string>
    {
        readonly ICloudinaryService _cloudinaryService;
        readonly IIssuePDFService _issuePDFService;

        public GeneratePDFForAnIssueCommandHandler(IApplicationDbContext context, ILogger<GeneratePDFForAnIssueCommand> logger, ICloudinaryService cloudinaryService, IIssuePDFService issuePDFService) : base(context, logger)
        {
            _cloudinaryService = cloudinaryService;
            _issuePDFService = issuePDFService;
        }

        public override async Task<string> Handle(GeneratePDFForAnIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues.Include(x => x.IssueProcessingHistories)
                .FirstAsync(x => x.Id == request._issueId, cancellationToken);

            // Generate PDF bytes using the GeneratePdfBytes method
            byte[] pdfBytes = _issuePDFService.GeneratePdfBytes(issue);

            var fileName = _issuePDFService.GetIssuePDFFileName(issue);

            // Create an UploadedFile object to represent the PDF file
            UploadedFile uploadedFile = new UploadedFile(fileName, pdfBytes);

            // Upload the PDF to Cloudinary using the CloudinaryService
            var fileData = await _cloudinaryService.UploadPdfAsync(uploadedFile, cancellationToken);

            var cloudinaryFile = new CloudinaryFile()
            {
                PublicId = fileData.PublicId,
                SecureUri = fileData.SecureUri.AbsoluteUri,
                FileName = fileData.FileName,
            };

            _ = await _context.CloudinaryFileIssues.AddAsync(new CloudinaryFileIssue() { Issue = issue, CloudinaryFile = cloudinaryFile, CloudinaryFileIssueType = CloudinaryFileIssueType.General }, cancellationToken);

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return fileData.SecureUri.AbsoluteUri;
        }
    }
}