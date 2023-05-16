namespace MR.Service.Features.SignaturePoolsFeatures.Queries;

public class GetSignaturePoolsBySearchParamsQuery : QueryBase, IRequest<PagedList<SignaturePool>>
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public string ApplicationUserEmail { get; set; } = string.Empty;
    public int? Year { get; set; }
    public int? Quarter { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public class GetSignaturePoolsByQueryHandler : CommandHandlerBase<GetSignaturePoolsBySearchParamsQuery, PagedList<SignaturePool>>
    {
        public GetSignaturePoolsByQueryHandler(IApplicationDbContext context, ILogger<GetSignaturePoolsBySearchParamsQuery> logger) :
            base(context, logger)
        {
        }

        public override async Task<PagedList<SignaturePool>> Handle(GetSignaturePoolsBySearchParamsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.SignaturePools.Include(x=>x.Signatures).Include(x=>x.Quarter).Include(x=>x.ApplicationUser).AsQueryable();

            if (!string.IsNullOrEmpty(request.ApplicationUserId))
            {
                query = query.Where(x => x.ApplicationUserId == request.ApplicationUserId);
            }

            if(!string.IsNullOrEmpty(request.ApplicationUserEmail))
            {
                query = query.Where(x => x.ApplicationUser.Email == request.ApplicationUserEmail);
            }

            if (request.Year.HasValue)
            {
                query = query.Where(p => p.Quarter.Year == request.Year.Value);
            }

            if (request.Quarter.HasValue)
            {
                query = query.Where(p => p.Quarter.Month == request.Quarter.Value);
            }

            if (request.Begin.HasValue)
            {
                var beginYear = request.Begin.Value.Year;
                var beginMonth = request.Begin.Value.Month;
                query = query.Where(p => (p.Quarter.Year > beginYear) || (p.Quarter.Year == beginYear && p.Quarter.Month >= beginMonth));
            }

            if (request.End.HasValue)
            {
                var endYear = request.End.Value.Year;
                var endMonth = request.End.Value.Month;
                query = query.Where(p => (p.Quarter.Year < endYear) || (p.Quarter.Year == endYear && p.Quarter.Month <= endMonth));
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);


            var pagedList = await PagedList<SignaturePool>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }
    }
}
