namespace MR.Service.Features.QuarterFeatures.Queries;

public class GetQuartersBySearchParamsQuery : QueryBase, IRequest<PagedList<Quarter>>
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public class GetQuartersByQueryHandler : CommandHandlerBase<GetQuartersBySearchParamsQuery, PagedList<Quarter>>
    {
        public GetQuartersByQueryHandler(IApplicationDbContext context, ILogger<GetQuartersBySearchParamsQuery> logger) : 
            base(context, logger)
        {
        }

        public override async Task<PagedList<Quarter>> Handle(GetQuartersBySearchParamsQuery request, 
            CancellationToken cancellationToken)
        {
            var query = _context.Quarters.AsQueryable();

            if (request.Year.HasValue)
            {
                query = query.Where(p => p.Year == request.Year.Value);
            }

            if (request.Month.HasValue)
            {
                query = query.Where(p => p.Month == request.Month.Value);
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);


            var pagedList = await PagedList<Quarter>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}
