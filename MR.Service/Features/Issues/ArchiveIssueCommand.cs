namespace MR.Service.Features.Issues;

public class ArchiveIssueCommand : IRequest<bool>
{
    private readonly Guid _issueId;
    public ArchiveIssueCommand(Guid issueId)
    {
        _issueId = issueId;
    }

    internal class ArchiveIssueCommandHandler : CommandHandlerBase<ArchiveIssueCommand, bool>
    {
        public ArchiveIssueCommandHandler(IApplicationDbContext context, ILogger<ArchiveIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(ArchiveIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues.FirstOrDefaultAsync(x => x.Id == request._issueId, cancellationToken);

            if (issue == null)
            {
                return false;
            }

            issue.IsDeleted = true;

            var sum = await _context.SaveChangesAsync(cancellationToken) > 0;
            return sum;
        }
    }
}