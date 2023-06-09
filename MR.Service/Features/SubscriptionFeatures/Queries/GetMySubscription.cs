namespace MR.Service.Features.SubscriptionFeatures.Queries;

public class GetMySubscription : IRequest<Subscription>
{
    public string ApplicationUserId { get; }
    public GetMySubscription(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }
    public class GetMySubscriptionHandler : CommandHandlerBase<GetMySubscription, Subscription>
    {
        public GetMySubscriptionHandler(IApplicationDbContext context, ILogger<GetMySubscription> logger) : base(context, logger)
        {
        }

        public override async Task<Subscription> Handle(GetMySubscription request, CancellationToken cancellationToken)
        {
            var subscription = await _context.Subscriptions
                .Include(x => x.ApplicationUser)
                .Include(x => x.SubscriptionPayments).ThenInclude(sp => sp.Payment).ThenInclude(spH => spH.PaymentStatusHistories)
                .FirstOrDefaultAsync(x => x.ApplicationUserId == request.ApplicationUserId, cancellationToken);

            if (subscription == null)
            {
                throw new ApplicationException("User doesn't have a subscription, contact admin to add it");
            }

            return subscription;
        }
    }
}