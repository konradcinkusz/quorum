namespace MR.Service.Features.SubscriptionFeatures;

public sealed class ActivateSubscriptionCommand : IRequest<bool>
{
    private readonly string _applicationUserId;
    public ActivateSubscriptionCommand(string applicationUserId)
    {
        _applicationUserId = applicationUserId;
    }

    internal class ActivateSubscriptionCommandHandler : CommandHandlerBase<ActivateSubscriptionCommand, bool>
    {
        public ActivateSubscriptionCommandHandler(IApplicationDbContext context, ILogger<ActivateSubscriptionCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var currentDate = DateTime.UtcNow;

            var subscriptionPayment = await _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .FirstOrDefaultAsync(x => x.Payment.PaymentStatus == PaymentStatus.Accepted && x.Subscription.ApplicationUserId == request._applicationUserId, cancellationToken);

            if (subscriptionPayment != null && !subscriptionPayment.Subscription.IsActive())
            {
                subscriptionPayment.Subscription.Begin = currentDate;
                subscriptionPayment.Subscription.End = currentDate.AddYears(1);
                subscriptionPayment.Payment.PaymentStatus = PaymentStatus.Completed;
                subscriptionPayment.Payment.PaymentStatusHistories = new List<PaymentStatusHistory>()
                    {
                        new PaymentStatusHistory {
                            PaymentStatus = PaymentStatus.Completed
                        }
                    };
            }

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return sum > 0;
        }
    }
}