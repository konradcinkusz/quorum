namespace Quorum.Service.Features.Issues.Queries;

public class GetCurrentQuarterPublishedIssues : QueryBase, IRequest<PagedList<Issue>>
{
    internal class GetCurrentQuarterPublishedIssuesHandler : CommandQueryHandlerBase<GetCurrentQuarterPublishedIssues, PagedList<Issue>>
    {
        public GetCurrentQuarterPublishedIssuesHandler(IApplicationDbContext context, ILogger<GetCurrentQuarterPublishedIssues> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetCurrentQuarterPublishedIssues request, CancellationToken cancellationToken)
        {
            var query = _context.Issues
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueVisibilityHistories)
                .Include(x => x.IssueProcessingHistories)
                .Where(x => 
                        !x.IsDeleted && 
                        x.IsVerifyByAdmin && 
                        x.InitialPayment != null &&
                        x.IssueProcess == IssueProcess.Published &&
                        x.IssueVisibility == IssueVisibility.VisibleForAll);

            var currentDate = DateTime.UtcNow;
            var currentYear = currentDate.Year;
            var currentQuarter = (currentDate.Month - 1) / 3 + 1;
            query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.Year == currentYear));
            query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.QuarterNumber == currentQuarter));

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<Issue>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}
