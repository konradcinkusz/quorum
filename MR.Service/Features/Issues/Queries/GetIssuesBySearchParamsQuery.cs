namespace MR.Service.Features.Issues.Queries;

public class GetIssuesBySearchParamsQuery : QueryBase, IRequest<PagedList<Issue>>
{
    public string? CreatedByEmail { get; set; }
    public class GetIssuesBySearchParamsQueryHandler : CommandHandlerBase<GetIssuesBySearchParamsQuery, PagedList<Issue>>
    {
        public GetIssuesBySearchParamsQueryHandler(IApplicationDbContext context, ILogger<GetIssuesBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetIssuesBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Issues.Include(x=>x.CreatedBy).AsQueryable();

            if (!string.IsNullOrEmpty(request.CreatedByEmail))
            {
                query = query.Where(x => x.CreatedBy != null && !string.IsNullOrEmpty(x.CreatedBy.Email) &&
                            x.CreatedBy.Email.Contains(request.CreatedByEmail));
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<Issue>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }

        protected override IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortColumn, SortOrder sortOrder)
        {
            if (!string.IsNullOrEmpty(sortColumn) && sortOrder != SortOrder.Unspecified)
            {
                switch (sortColumn)
                {
                    case "ApplicationUserEmail":
                        if (sortOrder == SortOrder.Ascending)
                        {
                            query = query.OrderBy(p => (p as Issue).CreatedBy.Email);
                        }
                        else if (sortOrder == SortOrder.Descending)
                        {
                            query = query.OrderByDescending(p => (p as Issue).CreatedBy.Email);
                        }
                        break;
                    default:
                        query = base.ApplySorting(query, sortColumn, sortOrder);
                        break;
                }
            }
            return query;
        }
    }
}
