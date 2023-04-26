namespace MR.Service.Features.SubscriptionFeatures;

public class BuySubscriptionCommand : ISubscriptionBaseCommand, IRequest<Guid>
{
    public Guid? PaymentId { get; set; }
    public string ApplicationUserId { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public decimal Price { get; set; }

    public class BuySubscriptionCommandHandler : CommandHandlerBase<BuySubscriptionCommand, Guid>
    {
        public BuySubscriptionCommandHandler(IApplicationDbContext context, ILogger<BuySubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<Guid> Handle(BuySubscriptionCommand request, CancellationToken cancellationToken)
        {
            var activeSubscriptionExists = _context.Subscriptions
                .Where(s => s.ApplicationUserId == request.ApplicationUserId);

            if (await activeSubscriptionExists.AnyAsync(x => x.IsActive()))
            {
                throw new ApplicationException("User already has an active subscription");
            }

            var subscription = new Subscription
            {
                ApplicationUserId = request.ApplicationUserId,
                Begin = request.Begin,
                End = request.End
            };

            await _context.Subscriptions.AddAsync(subscription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return subscription.Id;
        }
    }
}
