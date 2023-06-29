namespace MR.Service.Features.Issues;

public class GetIssueByIdForEdit : IRequest<Issue>
{
    public Guid Id { get; }

    public GetIssueByIdForEdit(Guid id)
    {
        Id = id;
    }

    public class GetIssueByIdForEditHandler : CommandQueryHandlerBase<GetIssueByIdForEdit, Issue>
    {
        public GetIssueByIdForEditHandler(IApplicationDbContext context, ILogger<GetIssueByIdForEdit> logger) : base(context, logger)
        {
        }

        public override async Task<Issue> Handle(GetIssueByIdForEdit request, CancellationToken cancellationToken)
        {
            return await _context.Issues
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueVisibilityHistories)
                .Include(x => x.IssueProcessingHistories)
                .Include(x => x.CreatedBy)
                .FirstAsync(x=>x.Id == request.Id, cancellationToken);
        }
    }
}
