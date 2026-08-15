namespace Quorum.Service.Features.Issues;

/// <summary>
/// Delete od force delete różni się tym, że nie usuwamy powiązań -> patrz DeleteQuarter
/// W force delete usuwamy wszystkie powiązane obiekty -> patrz na ForceDeleteIssueCommand
/// </summary>
public class ForceDeleteIssueCommand : IRequest<bool>
{
    private readonly Guid _issueId;
    public ForceDeleteIssueCommand(Guid issueId)
    {
        _issueId = issueId;
    }

    internal class ForceDeleteIssueCommandHandler : CommandHandlerBase<ForceDeleteIssueCommand, bool>
    {
        public ForceDeleteIssueCommandHandler(IApplicationDbContext context, ILogger<ForceDeleteIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(ForceDeleteIssueCommand request, CancellationToken cancellationToken)
        {
            bool result = false;
            var issue = await _context.Issues
                .Include(x => x.InitialPayment)
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
                if (issue.InitialPayment != null)
                {
                    _context.Payments.Remove(issue.InitialPayment);
                }
                _context.Issues.Remove(issue);
                result = await _context.SaveChangesAsync(cancellationToken) > 0;
            }

            return result;
        }
    }
}
