namespace MR.Service.Features.SignatureFeautres;

public class SignIssueCommand : IRequest<bool>
{
    private readonly Guid _issueId;
    private readonly string _applicationUserId;
    public SignIssueCommand(Guid issueId, string applicationUserId)
    {
        _issueId = issueId;
        _applicationUserId = applicationUserId;
    }

    internal class SignIssueCommandHandler : CommandHandlerBase<SignIssueCommand, bool>
    {
        public SignIssueCommandHandler(IApplicationDbContext context, ILogger<SignIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(SignIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues.FirstOrDefaultAsync(x => x.Id == request._issueId, cancellationToken);

            if (issue == null)
            {
                throw new ArgumentNullException($"There are no issue with provided Guid: {request._issueId}");
            }

            var signaturePool = await _context.SignaturePools
                .Include(x => x.ApplicationUser)
                .Include(x => x.Quarter)
                .Include(x => x.Signatures).ThenInclude(x => x.Issue)
                .Where(x => x.ApplicationUserId == request._applicationUserId).ToListAsync(cancellationToken);

            if (signaturePool == null)
            {
                throw new InvalidOperationException("You don't have any signature pool. Contact Administrator");
            }

            //Check if issue has been already signed by the user:
            if (signaturePool.Any(x => x.Signatures.Any(y => y.IssueId == request._issueId)))
            {
                throw new InvalidOperationException("Issue has been already signed by the user");
            }

            var signature = signaturePool.FirstOrDefault(x => QuarterExtensions.CheckCurrentQuarter(x.Quarter))?.Signatures.FirstOrDefault(x => x.Issue == null);
            if (signature == null)
            {
                throw new Exception("You have alread used your all signature within current signature pool");
            }

            signature.Issue = issue;
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}