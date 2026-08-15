namespace Quorum.Service.Features.Issues.Queries;

public class GetSignedIssuesCommand : QueryBase, IRequest<PagedList<Issue>>
{
    internal class GetSignedIssuesCommandHandler : CommandQueryHandlerBase<GetSignedIssuesCommand, PagedList<Issue>>
    {
        public GetSignedIssuesCommandHandler(IApplicationDbContext context, ILogger<GetSignedIssuesCommand> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetSignedIssuesCommand request, CancellationToken cancellationToken)
        {
            var query = _context.SignaturePools.Include(x => x.Signatures).ThenInclude(x => x.Issue).Include(x => x.Quarter).AsQueryable();

            query =  ApplyUserFilter(query, request);

            var currentDate = DateTime.UtcNow;
            var currentYear = currentDate.Year;
            var currentQuarter = (currentDate.Month - 1) / 3 + 1;

            query = query.Where(y => y.Quarter.Year == currentYear);
            query = query.Where(y => y.Quarter.QuarterNumber == currentQuarter);

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            // Select and filter the Issues from Signatures
            var issues = query
                .SelectMany(x => x.Signatures.Where(s => s.Issue != null).Select(s => s.Issue));

            var pagedList = await PagedList<Issue>.CreateAsync(issues, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}
