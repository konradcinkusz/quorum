namespace MR.Service.Features.SignaturePoolsFeatures.Queries;

public class GetSignaturePoolsBySearchParamsQuery : QueryBase, IRequest<PagedList<SignaturePool>>
{
    public int? Year { get; set; }
    public int? Quarter { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public class GetSignaturePoolsByQueryHandler : CommandQueryHandlerBase<GetSignaturePoolsBySearchParamsQuery, PagedList<SignaturePool>>
    {
        public GetSignaturePoolsByQueryHandler(IApplicationDbContext context, ILogger<GetSignaturePoolsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<SignaturePool>> Handle(GetSignaturePoolsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SignaturePools
                .Include(x=>x.Signatures).Include(x=>x.Quarter).Include(x=>x.ApplicationUser).AsQueryable();

            query = ApplyUserFilter(query, request);

            if (request.Year.HasValue)
            {
                query = query.Where(p => p.Quarter.Year == request.Year.Value);
            }

            if (request.Quarter.HasValue)
            {
                query = query.Where(p => p.Quarter.QuarterNumber == request.Quarter.Value);
            }

            if (request.Begin.HasValue)
            {
                var beginYear = request.Begin.Value.Year;
                var beginMonth = request.Begin.Value.Month;
                query = query.Where(p => (p.Quarter.Year > beginYear) || (p.Quarter.Year == beginYear && p.Quarter.QuarterNumber >= beginMonth));
            }

            if (request.End.HasValue)
            {
                var endYear = request.End.Value.Year;
                var endMonth = request.End.Value.Month;
                query = query.Where(p => (p.Quarter.Year < endYear) || (p.Quarter.Year == endYear && p.Quarter.QuarterNumber <= endMonth));
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<SignaturePool>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }

        protected override IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortColumn, SortOrder sortOrder) 
        {
            if (!string.IsNullOrEmpty(sortColumn) && sortOrder != SortOrder.Unspecified)
            {
                switch (sortColumn)
                {
                    case "Year":
                        if (sortOrder == SortOrder.Ascending)
                        {
                            query = query.OrderBy(p => (p as SignaturePool).Quarter.Year);
                        }
                        else if (sortOrder == SortOrder.Descending)
                        {
                            query = query.OrderByDescending(p => (p as SignaturePool).Quarter.Year);
                        }
                        break;
                    case "Month":
                        if (sortOrder == SortOrder.Ascending)
                        {
                            query = query.OrderBy(p => (p as SignaturePool).Quarter.QuarterNumber);
                        }
                        else if (sortOrder == SortOrder.Descending)
                        {
                            query = query.OrderByDescending(p => (p as SignaturePool).Quarter.QuarterNumber);
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
