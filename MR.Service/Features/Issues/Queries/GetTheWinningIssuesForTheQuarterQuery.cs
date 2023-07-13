namespace MR.Service.Features.Issues.Queries;

public class GetTheWinningIssuesForTheQuarterQuery : QueryBase, IRequest<PagedList<Issue>>
{
    public int? QuarterYear { get; set; }
    public int? QuarterNumber { get; set; }

    internal class GetTheWinningIssuesForTheQuarterHandler : CommandQueryHandlerBase<GetTheWinningIssuesForTheQuarterQuery, PagedList<Issue>>
    {
        public GetTheWinningIssuesForTheQuarterHandler(IApplicationDbContext context, ILogger<GetTheWinningIssuesForTheQuarterQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetTheWinningIssuesForTheQuarterQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Issues
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueRatingHistories)
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsVerifyByAdmin &&
                    x.InitialPayment != null &&
                    x.IssueProcess == IssueProcess.EndedInCurrentQuarter &&
                    x.IssueVisibility == IssueVisibility.VisibleForAll &&
                    x.QuarterIssues.Any(y => y.QuarterWinner.HasValue && y.QuarterWinner.Value));

            if (request.QuarterYear.HasValue)
            {
                query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.Year == request.QuarterYear.Value));
            }

            if (request.QuarterNumber.HasValue)
            {
                query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.QuarterNumber == request.QuarterNumber.Value));
            }

            var pagedList = await PagedList<Issue>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}