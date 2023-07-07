namespace MR.Service.Features.Issues;

public class ForceDeleteCommand : IRequest<bool>
{
    private readonly Guid _issueId;
    public ForceDeleteCommand(Guid issueId)
    {
        _issueId = issueId;
    }

    internal class ForceDeleteCommandHandler : CommandHandlerBase<ForceDeleteCommand, bool>
    {
        public ForceDeleteCommandHandler(IApplicationDbContext context, ILogger<ForceDeleteCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(ForceDeleteCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues
                .Include(x => x.IssueProcessingHistories)
                .Include(x => x.IssueVisibilityHistories)
                .Include(x => x.QuarterIssues)
                .Include(x => x.Signatures)
                .FirstOrDefaultAsync(x => x.Id == request._issueId, cancellationToken);

            if (issue != null)
            {
                _context.IssueProcessingHistories.RemoveRange(issue.IssueProcessingHistories);
                _context.IssueVisibilityHistories.RemoveRange(issue.IssueVisibilityHistories);
                _context.QuarterIssues.RemoveRange(issue.QuarterIssues);
                _context.Signatures.RemoveRange(issue.Signatures);

                _context.Issues.Remove(issue);
                var sum = await _context.SaveChangesAsync(cancellationToken);
                return sum > 0;
            }

            return false;
        }
    }
}