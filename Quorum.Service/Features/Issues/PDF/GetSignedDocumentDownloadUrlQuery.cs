namespace Quorum.Service.Features.Issues.PDF;

/// <summary>
/// Returns a short-lived, signed URL for the signed petition sheet the calling user attached
/// to an issue.
/// </summary>
/// <remarks>
/// <para>
/// This is not new reach. Before it existed, <see cref="UploadSignedDocumentCommand"/> handed
/// the caller a <b>permanent, unauthenticated</b> CDN link to the same document as its return
/// value. This replaces that with a URL that expires, and puts the eligibility check in front
/// of it — the same check, by the same code, that decided the upload was allowed in the first
/// place.
/// </para>
/// <para>
/// Without it, making delivery authenticated would leave the documents unreachable by anyone,
/// which is not a fix.
/// </para>
/// </remarks>
public class GetSignedDocumentDownloadUrlQuery : IRequest<string>
{
    readonly Guid _issueId;
    readonly string _applicationUserId;

    public GetSignedDocumentDownloadUrlQuery(Guid issueId, string applicationUserId)
    {
        _issueId = issueId;
        _applicationUserId = applicationUserId;
    }

    internal class GetSignedDocumentDownloadUrlQueryHandler : CommandHandlerBase<GetSignedDocumentDownloadUrlQuery, string>
    {
        readonly ICloudinaryService _cloudinaryService;

        public GetSignedDocumentDownloadUrlQueryHandler(IApplicationDbContext context, ILogger<GetSignedDocumentDownloadUrlQuery> logger, ICloudinaryService cloudinaryService) : base(context, logger)
        {
            _cloudinaryService = cloudinaryService;
        }

        public override async Task<string> Handle(GetSignedDocumentDownloadUrlQuery request, CancellationToken cancellationToken)
        {
            // Eligibility first, and by the same rule the upload used. Resolving the issue
            // through RestrictToSignatory rather than looking the file up directly is what
            // makes "may fetch" and "may attach" the same question: a user who has since
            // stopped qualifying -- the issue moved on, or lost its visibility -- stops being
            // able to pull the document down, without anyone having to remember to say so
            // here.
            var eligibleIssueId = await _context.Issues
                .Where(x => x.Id == request._issueId)
                .RestrictToSignatory(request._applicationUserId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (eligibleIssueId == Guid.Empty)
            {
                throw new NotFoundException(nameof(Issue), request._issueId);
            }

            // Scoped to this user's own upload as well. Several signatories can attach a
            // sheet to one issue, and being eligible for the issue does not make someone
            // eligible for another person's signatures.
            var file = await _context.CloudinaryFileIssues
                .Where(x => x.IssueId == eligibleIssueId)
                .Where(x => x.CloudinaryFileIssueType == CloudinaryFileIssueType.UserSigned)
                .Where(x => x.ApplicationUserId == request._applicationUserId)
                .Select(x => x.CloudinaryFile)
                .FirstOrDefaultAsync(cancellationToken);

            // Same NotFound as an ineligible caller gets, so the response cannot be used to
            // tell "you may not see this" apart from "there is nothing here".
            if (file == null)
            {
                throw new NotFoundException(nameof(Issue), request._issueId);
            }

            return _cloudinaryService.BuildSignedDownloadUrl(
                file.PublicId, SignedDocumentRules.DownloadUrlLifetime);
        }
    }
}
