namespace MR.Service.Features.SubscriptionFeatures.Queries;

public class GetSubscriptionsBySearchParamsQuery : QueryBase, IRequest<PagedList<Subscription>>
{
    public Guid SubscriptionId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public bool OnlyActives { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }

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
                query = query.Where(x => x.IsActive());
            }

            if (request.BeginDate != null)
            {
                query = query.Where(p => p.Begin >= request.BeginDate);
            }

            if (request.EndDate != null)
            {
                query = query.Where(p => p.End <= request.EndDate);
            }
            return new PagedList<Subscription>(query, request.SearchParams);
        }
    }
}
