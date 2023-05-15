namespace MR.Service.Features.SubscriptionFeatures.Queries;

public class GetSubscriptionsBySearchParamsQuery : QueryBase, IRequest<PagedList<Subscription>>, ISubscriptionBaseCommand
{
    public string ApplicationUserId { get; set; }
    public bool OnlyActives { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public class GetSubscriptionsBySearchParamsHandler : CommandHandlerBase<GetSubscriptionsBySearchParamsQuery, PagedList<Subscription>>
    {
        public GetSubscriptionsBySearchParamsHandler(IApplicationDbContext context, ILogger<GetSubscriptionsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Subscription>> Handle(GetSubscriptionsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Subscriptions
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(request.ApplicationUserId))
            {
                query = query.Where(p => p.ApplicationUserId == request.ApplicationUserId);
            }
            
            if (request.OnlyActives)
            {
                var currentDate = DateTime.UtcNow;
                query = query.Where(x => currentDate >= x.Begin && currentDate <= x.End);
            }

            if (request.Begin != null)
            {
                query = query.Where(p => p.Begin >= request.Begin);
            }

            if (request.End != null)
            {
                query = query.Where(p => p.End <= request.End);
            }

            return await PagedList<Subscription>.CreateAsync(query, request.SearchParams, cancellationToken);
        }
    }
}
