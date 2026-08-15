namespace Quorum.Service.Features.SignatureFeautres;

public class UnpinSignatureFromIssueCommand : IRequest<bool>
{
    private readonly Guid _signatureId;
    public UnpinSignatureFromIssueCommand(Guid signatureId)
    {
        _signatureId = signatureId;
    }

    internal class UnpinIssueCommandHandler : CommandHandlerBase<UnpinSignatureFromIssueCommand, bool>
    {
        public UnpinIssueCommandHandler(IApplicationDbContext context, ILogger<UnpinSignatureFromIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(UnpinSignatureFromIssueCommand request, CancellationToken cancellationToken)
        {
            bool result = false;
            var signature = await _context.Signatures.Include(x => x.Issue).FirstOrDefaultAsync(x => x.Id == request._signatureId, cancellationToken);

            if (signature != null)
            {
                var issue = signature.Issue;
                if (issue != null)
                {
                    _ = await _context.IssueRatingHistories.AddAsync(new IssueRatingHistory() { Issue = issue, Value = -1, Action = RatingAction.UnpinSignatureByAdmin, RelatedObject = $"Signature: {request._signatureId} unpinned by admin from the IssueId: {issue.Id}" }, cancellationToken);
                }

                signature.Issue = null;
                result = await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            return result;
        }
    }
}