namespace MR.Service.Features.Issues;

public class ArchiveIssueCommand : IRequest<bool>
{
    private readonly Guid _issueId;
    private readonly IssueOwnerScope _scope;

    /// <param name="scope">
    /// Which issues this caller may archive. Use <see cref="IssueOwnerScope.OwnedBy"/> for
    /// a user-facing route; administrators have <c>force-delete-issue</c> instead.
    /// </param>
    public ArchiveIssueCommand(Guid issueId, IssueOwnerScope scope)
    {
        _issueId = issueId;
        _scope = scope;
    }

    internal class ArchiveIssueCommandHandler : CommandHandlerBase<ArchiveIssueCommand, bool>
    {
        public ArchiveIssueCommandHandler(IApplicationDbContext context, ILogger<ArchiveIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(ArchiveIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues
                .RestrictToOwner(request._scope)
                .FirstOrDefaultAsync(x => x.Id == request._issueId, cancellationToken);

            // Same result whether the issue does not exist or belongs to someone else.
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