namespace MR.Service.Features.SignaturePoolsFeatures.Queries;

public class GetUserSignaturePools : QueryBase, IRequest<PagedList<SignaturePool>>
{
    public int? Year { get; set; }
    public int? Quarter { get; set; }

    public GetUserSignaturePools()
    {
    }

    internal class GetUserSignaturePoolsCommand : CommandQueryHandlerBase<GetUserSignaturePools, PagedList<SignaturePool>>
    {
        public GetUserSignaturePoolsCommand(IApplicationDbContext context, ILogger<GetUserSignaturePools> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<SignaturePool>> Handle(GetUserSignaturePools request, CancellationToken cancellationToken)
        {
            var query = _context.SignaturePools.Include(x => x.Signatures).ThenInclude(x=>x.Issue).Include(x => x.Quarter).AsQueryable();

            query = ApplyUserFilter(query, request);

            if (request.Year.HasValue)
            {
                query = query.Where(p => p.Quarter.Year == request.Year.Value);
            }

            if (request.Quarter.HasValue)
            {
                query = query.Where(p => p.Quarter.QuarterNumber == request.Quarter.Value);
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<SignaturePool>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}
