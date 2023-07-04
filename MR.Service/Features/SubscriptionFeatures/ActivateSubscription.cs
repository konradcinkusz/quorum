using MR.Service.ViewModels;

namespace MR.Service.Features.SubscriptionFeatures;

public class ActivateSubscriptionCommand : IRequest<PagedList<Subscription>>
{
    public class ActivateSubscriptionCommandHandler : 
        CommandHandlerBase<ActivateSubscriptionCommand, PagedList<Subscription>>
    {
        public ActivateSubscriptionCommandHandler(
            IApplicationDbContext context, ILogger<ActivateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<PagedList<Subscription>> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var currentDate = DateTime.UtcNow;

            var query = await _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .Where(x => x.Payment.PaymentStatus == PaymentStatus.Accepted)
                .ToListAsync(cancellationToken);

            foreach (var item in query)
            {
                if (!item.Subscription.IsActive())
                {
                    item.Subscription.Begin = currentDate;
                    item.Subscription.End = currentDate.AddYears(1);
                    item.Payment.PaymentStatus = PaymentStatus.Completed;
                    item.Payment.PaymentStatusHistories = new List<PaymentStatusHistory>() 
                    { 
                        new PaymentStatusHistory {
                            PaymentStatus = PaymentStatus.Completed
                        } 
                    };
                }
            }
            
            var sum = await _context.SaveChangesAsync(cancellationToken);

            return PagedList<Subscription>.Create(query.Select(x => x.Subscription).ToList(), new());
        }
    }
}
