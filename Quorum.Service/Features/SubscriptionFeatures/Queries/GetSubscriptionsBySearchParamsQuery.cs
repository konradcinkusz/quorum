using Quorum.Service.ViewModels;

namespace Quorum.Service.Features.SubscriptionFeatures.Queries;

public class GetSubscriptionsBySearchParamsQuery : QueryBase, IRequest<PagedList<Subscription>>
{
    public enum ActivityEnum
    {
        All,
        Active,
        InActive
    }
    public ActivityEnum? Activity { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public class GetSubscriptionsBySearchParamsHandler : CommandQueryHandlerBase<GetSubscriptionsBySearchParamsQuery, PagedList<Subscription>>
    {
        public GetSubscriptionsBySearchParamsHandler(IApplicationDbContext context, ILogger<GetSubscriptionsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Subscription>> Handle(GetSubscriptionsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query =
                _context.Subscriptions
                .Include(x => x.SubscriptionPayments).ThenInclude(sp => sp.Payment).ThenInclude(spH => spH.PaymentStatusHistories)
                .AsQueryable();

            query = ApplyUserFilter(query, request);

            if (request.Activity.HasValue && request.Activity.Value != ActivityEnum.All)
            {
                var currentDate = DateTime.UtcNow;
                switch (request.Activity.Value)
                {
                    case ActivityEnum.Active:
                        query = query.Where(x => currentDate >= x.Begin && currentDate <= x.End);
                        break;
                    case ActivityEnum.InActive:
                        query = query.Where(x => currentDate <= x.Begin && currentDate >= x.End);
                        break;
                }
            }

            if (request.Begin != null)
            {
                query = query.Where(p => p.Begin >= request.Begin);
            }

            if (request.End != null)
            {
                query = query.Where(p => p.End <= request.End);
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<Subscription>.CreateAsync(query, request.SearchParams, cancellationToken);

            await _context.PopulateUserEmailsAsync(
                pagedList, x => x.ApplicationUserId, (x, email) => x.ApplicationUserEmail = email, cancellationToken);

            return pagedList;
        }
    }
}
