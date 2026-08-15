namespace Quorum.Service.Features.Issues.Queries;

public class GetIssueByIdForEdit : IRequest<Issue>
{
    public Guid Id { get; }
    public IssueOwnerScope Scope { get; }

    /// <param name="scope">
    /// Which issues this caller may load for editing. Use
    /// <see cref="IssueOwnerScope.OwnedBy"/> for a user-facing route and
    /// <see cref="IssueOwnerScope.Administrator"/> only from a route already gated behind
    /// the admin policy.
    /// </param>
    public GetIssueByIdForEdit(Guid id, IssueOwnerScope scope)
    {
        Id = id;
        Scope = scope;
    }

    public class GetIssueByIdForEditHandler : CommandQueryHandlerBase<GetIssueByIdForEdit, Issue>
    {
        public GetIssueByIdForEditHandler(IApplicationDbContext context, ILogger<GetIssueByIdForEdit> logger) : base(context, logger)
        {
        }

        public override async Task<Issue> Handle(GetIssueByIdForEdit request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues
                .RestrictToOwner(request.Scope)
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueVisibilityHistories)
                .Include(x => x.IssueProcessingHistories)
                .Include(x => x.CreatedBy)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Same result whether the issue does not exist or belongs to someone else, so
            // this endpoint cannot be used to enumerate other users' issue ids.
            if (issue == null)
            {
                throw new NotFoundException(nameof(Issue), request.Id);
            }

            return issue;
        }
    }
}
