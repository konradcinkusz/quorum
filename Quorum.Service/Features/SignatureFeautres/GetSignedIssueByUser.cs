namespace Quorum.Service.Features.SignatureFeautres;

public class GetSignedIssueByUser : IRequest<List<Guid>>
{
    private readonly string _applicationUserId;
    public GetSignedIssueByUser(string applicationUserId)
    {
        _applicationUserId = applicationUserId;
    }

    internal class GetSignedIssueByUserHandler : CommandHandlerBase<GetSignedIssueByUser, List<Guid>>
    {
        public GetSignedIssueByUserHandler(IApplicationDbContext context, ILogger<GetSignedIssueByUser> logger) : base(context, logger)
        {
        }

        public override async Task<List<Guid>> Handle(GetSignedIssueByUser request, CancellationToken cancellationToken)
        {
            var issues = await _context.SignaturePools
                .Include(x => x.Signatures).ThenInclude(x => x.Issue)
                .Where(x => x.ApplicationUserId == request._applicationUserId)
                .SelectMany(x => x.Signatures.Select(x => x.Issue)).ToListAsync(cancellationToken);

            return issues.Where(x => x != null).Select(x => x.Id).ToList();
        }
    }
}
