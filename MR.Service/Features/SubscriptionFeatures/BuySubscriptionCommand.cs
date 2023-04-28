namespace MR.Service.Features.SubscriptionFeatures;

public class BuySubscriptionCommand : IRequest<bool>
{
    public string ApplicationUserId { get; set; }

    public class BuySubscriptionCommandHandler : CommandHandlerBase<BuySubscriptionCommand, bool>
    {
        public BuySubscriptionCommandHandler(IApplicationDbContext context, ILogger<BuySubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<bool> Handle(BuySubscriptionCommand request, CancellationToken cancellationToken)
        {
            var activeSubscriptionExists = await _context.Subscriptions.Include(x=>x.SubscriptionPayments).ThenInclude(x=>x.Payment)
                .FirstAsync(s => s.ApplicationUserId == request.ApplicationUserId);

            if (activeSubscriptionExists.IsActive())
            {
                throw new ApplicationException("User already has an active subscription");
            }

            if (activeSubscriptionExists.SubscriptionPayments.Any(x=> x.Payment != null && x.Payment.PaymentStatus == PaymentStatus.Pending))
            {
                throw new ApplicationException("User has pending payment for that subscription");
            }

            Payment payment = new Payment
            {
                ApplicationUserId = request.ApplicationUserId,
                PaymentMethod = "Bank",
                PaymentStatus = PaymentStatus.Pending,
                PaymentValuePLN = 5
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
