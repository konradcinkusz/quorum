namespace MR.Service.Features.SubscriptionFeatures;

public class DeactivateSubscriptionCommand : IRequest<PagedList<Subscription>>
{
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
            var currentDate = DateTime.UtcNow.AddDays(-1);

            var query = await _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .Where(x => x.Payment.PaymentStatus == PaymentStatus.Accepted).ToListAsync(cancellationToken);

            foreach (var item in query)
            {
                if (item.Subscription.IsActive())
                {
                    item.Subscription.End = currentDate;
                }
            }

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return await PagedList<Subscription>.CreateAsync(query.Select(x => x.Subscription).AsQueryable(), new(), cancellationToken);
        }
    }
}
