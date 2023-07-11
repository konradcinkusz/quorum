namespace MR.Service.Features.SignatureFeautres;

public class UnsignIssueCommand : IRequest<bool>
{
    private readonly Guid _issueId;
    private readonly string _applicationUserId;
    public UnsignIssueCommand(Guid issueId, string applicationUserId)
    {
        _issueId = issueId;
        _applicationUserId = applicationUserId;
    }

    internal class UnsignIssueCommandHandler : CommandHandlerBase<UnsignIssueCommand, bool>
    {
        public UnsignIssueCommandHandler(IApplicationDbContext context, ILogger<UnsignIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(UnsignIssueCommand request, CancellationToken cancellationToken)
        {
            var signature = await _context.Signatures
                .Include(x => x.SignaturePool).ThenInclude(x => x.ApplicationUser)
                .Include(x => x.Issue)
                .FirstOrDefaultAsync(x => x.IssueId == request._issueId && x.SignaturePool.ApplicationUserId == request._applicationUserId, cancellationToken);

            if (signature == null)
            {
                throw new Exception($"There is no signature for unsign with Guid {request._issueId} for application user {request._applicationUserId}");
            }

            signature.Issue = null;
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}