namespace MR.Service.Features.QuarterFeatures.Queries;

public class GetQuartersBySearchParamsQuery : QueryBase, IRequest<PagedList<Quarter>>
{
    public int? Year { get; set; }
    public int? QuarterNumber { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public class GetQuartersByQueryHandler : CommandQueryHandlerBase<GetQuartersBySearchParamsQuery, PagedList<Quarter>>
    {
        public GetQuartersByQueryHandler(IApplicationDbContext context, ILogger<GetQuartersBySearchParamsQuery> logger) : 
            base(context, logger)
        {
        }

        public override async Task<PagedList<Quarter>> Handle(GetQuartersBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Quarters.AsQueryable();

            if (request.Year.HasValue)
            {
                query = query.Where(p => p.Year == request.Year.Value);
            }

            if (request.QuarterNumber.HasValue)
            {
                query = query.Where(p => p.QuarterNumber == request.QuarterNumber.Value);
            }

            if (request.Begin.HasValue)
            {
                var beginYear = request.Begin.Value.Year;
                var beginMonth = request.Begin.Value.Month;
                query = query.Where(p => (p.Year > beginYear) || (p.Year == beginYear && p.QuarterNumber >= beginMonth));
            }

            if (request.End.HasValue)
            {
                var endYear = request.End.Value.Year;
                var endMonth = request.End.Value.Month;
                query = query.Where(p => (p.Year < endYear) || (p.Year == endYear && p.QuarterNumber <= endMonth));
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<Quarter>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}
