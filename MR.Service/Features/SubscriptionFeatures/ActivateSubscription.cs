namespace MR.Service.Features.SubscriptionFeatures;

public class ActivateSubscriptionCommand : IRequest<int>
{
    public class ActivateSubscriptionCommandHandler : CommandHandlerBase<ActivateSubscriptionCommand, int>
    {
        public ActivateSubscriptionCommandHandler(
            IApplicationDbContext context, ILogger<ActivateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<int> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var query = _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .Where(x => x.Payment.PaymentStatus == PaymentStatus.Accepted).ToList();

            foreach (var item in query)
            {
                if (!item.Subscription.IsActive())
                {
                    item.Subscription.Begin = DateTime.UtcNow;
                    item.Subscription.End = DateTime.UtcNow.AddDays(365);
                    item.Payment.PaymentStatus = PaymentStatus.Completed;
                    item.Payment.PaymentStatusHistories = new List<PaymentStatusHistory>() 
                    { 
                        new PaymentStatusHistory {
                            PaymentStatus = PaymentStatus.Completed
                        } 
                    };
                }
            }
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
