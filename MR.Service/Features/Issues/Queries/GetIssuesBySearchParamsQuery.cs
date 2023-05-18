namespace MR.Service.Features.Issues.Queries;

public class GetIssuesBySearchParamsQuery : QueryBase, IRequest<PagedList<Issue>>
{
    public class GetIssuesBySearchParamsQueryHandler : CommandHandlerBase<GetIssuesBySearchParamsQuery, PagedList<Issue>>
    {
        public GetIssuesBySearchParamsQueryHandler(IApplicationDbContext context, ILogger<GetIssuesBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetIssuesBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Issues.AsQueryable();

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<Issue>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}
