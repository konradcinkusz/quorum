namespace MR.Service.Features.SubscriptionFeatures;

public class DeactivateSubscriptionCommand : IRequest<PagedList<Subscription>>
{
    public readonly string ApplicationUserId;
    public DeactivateSubscriptionCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }
    public class DeactivateSubscriptionCommandHandler :
        CommandHandlerBase<DeactivateSubscriptionCommand, PagedList<Subscription>>
    {
        public DeactivateSubscriptionCommandHandler(
            IApplicationDbContext context, ILogger<DeactivateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<PagedList<Subscription>> Handle(DeactivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var currentDate = DateTime.UtcNow.Date;

            var query = await _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .Where(x =>
                    x.Payment.PaymentStatus == PaymentStatus.Completed &&
                    x.Subscription.Begin.HasValue && x.Subscription.End.HasValue &&
                            x.Subscription.End.Value >= currentDate &&
                    x.SubscriptionId == request.ApplicationUserId)
                .ToListAsync(cancellationToken);


            foreach (var item in query)
            {
                if (item.Subscription.IsActive())
                {
                    item.Subscription.End = currentDate;
                }
            }

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return PagedList<Subscription>.Create(query.Select(x => x.Subscription).Distinct().ToList(), new());
        }
    }
}
