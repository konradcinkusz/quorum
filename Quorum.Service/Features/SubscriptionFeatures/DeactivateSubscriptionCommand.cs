namespace Quorum.Service.Features.SubscriptionFeatures;

public sealed class DeactivateSubscriptionCommand : IRequest<bool>
{
    public readonly string ApplicationUserId;
    public DeactivateSubscriptionCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }

    internal class DeactivateSubscriptionCommandHandler : CommandHandlerBase<DeactivateSubscriptionCommand, bool>
    {
        public DeactivateSubscriptionCommandHandler(
            IApplicationDbContext context, ILogger<DeactivateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<bool> Handle(DeactivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var currentDate = DateTime.UtcNow.Date;

            var subscriptionPayment = await _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .FirstOrDefaultAsync(x =>
                    x.Payment.PaymentStatus == PaymentStatus.Completed &&
                    x.Subscription.Begin.HasValue &&
                    x.Subscription.End.HasValue &&
                    x.Subscription.End.Value >= currentDate &&
                    x.SubscriptionId == request.ApplicationUserId, cancellationToken);

            if (subscriptionPayment != null && subscriptionPayment.Subscription.IsActive())
            {
                subscriptionPayment.Subscription.End = currentDate;
            }

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return sum > 0;
        }
    }
}
