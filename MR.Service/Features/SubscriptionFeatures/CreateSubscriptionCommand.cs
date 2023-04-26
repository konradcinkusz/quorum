namespace MR.Service.Features.SubscriptionFeatures;

public class CreateSubscriptionCommand : ISubscriptionBaseCommand, IRequest<Guid>
{
    public string ApplicationUserId { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public decimal Price { get; set; } = 0;

    public class CreateSubscriptionCommandHandler : CommandHandlerBase<CreateSubscriptionCommand, Guid>
    {
        public CreateSubscriptionCommandHandler(IApplicationDbContext context, ILogger<CreateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subscription = new Subscription
            {
                ApplicationUserId = request.ApplicationUserId,
                Begin = request.Begin,
                End = request.End
            };

            if (request.Price > 0)
            {
                var payment = new Payment
                {
                    ApplicationUserId = request.ApplicationUserId,
                    PaymentValuePLN = request.Price,
                    PaymentStatus = PaymentStatus.New
                };

                subscription.SubscriptionPayments = new List<SubscriptionPayment>
                {
                    new SubscriptionPayment
                    {
                        Subscription = subscription,
                        Payment = payment
                    }
                };

                await _context.Payments.AddAsync(payment, cancellationToken);
            }

            await _context.Subscriptions.AddAsync(subscription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return subscription.Id;
        }
    }
}
