namespace MR.Service.Features.SubscriptionFeatures;

public class BuySubscriptionCommand : IRequest<bool>
{
    public readonly string ApplicationUserId;

    public BuySubscriptionCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }

    public class BuySubscriptionCommandHandler : CommandHandlerBase<BuySubscriptionCommand, bool>
    {
        public BuySubscriptionCommandHandler(IApplicationDbContext context, ILogger<BuySubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<bool> Handle(BuySubscriptionCommand request, CancellationToken cancellationToken)
        {
            var userSub = 
                await _context.Subscriptions.Include(x=>x.SubscriptionPayments).ThenInclude(x=>x.Payment)
                .FirstOrDefaultAsync(s => s.ApplicationUserId == request.ApplicationUserId, cancellationToken);

            if (userSub == null)
            {
                throw new ApplicationException("User don't have subscription record, contact with admin. Subscription had to been added while registering.");
            }

            if (userSub.IsActive())
            {
                throw new ApplicationException("User already has an active subscription");
            }

            if (userSub.SubscriptionPayments.Any(x=> x.Payment != null 
                && x.Payment.PaymentStatus == PaymentStatus.Pending))
            {
                throw new ApplicationException("User has pending payment for that subscription");
            }

            userSub.Begin = null;
            userSub.End = null;

            Payment payment = new Payment
            {
                ApplicationUserId = request.ApplicationUserId,
                PaymentMethod = "Bank",
                ReferenceNumber = "any amount of money",
                PaymentStatus = PaymentStatus.Pending,
                PaymentValuePLN = 0.01M,
                PaymentStatusHistories = new List<PaymentStatusHistory>()
                    {
                        new PaymentStatusHistory {
                            PaymentStatus = PaymentStatus.New,
                        },
                        new PaymentStatusHistory {
                            PaymentStatus = PaymentStatus.Pending,
                        }
                    }
            };

            SubscriptionPayment subscriptionPayment = new SubscriptionPayment
            {
                SubscriptionId = request.ApplicationUserId
            };

            payment.SubscriptionPayments = new List<SubscriptionPayment>() { subscriptionPayment };

            await _context.Payments.AddAsync(payment, cancellationToken);

            int result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}
