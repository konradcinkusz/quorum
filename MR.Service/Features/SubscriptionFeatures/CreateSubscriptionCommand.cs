namespace MR.Service.Features.SubscriptionFeatures;

public class CreateSubscriptionCommand : IRequest<Guid>
{
    public Guid PaymentId { get; set; }
    public string ApplicationUserId { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public class CreateSubscriptionCommandHandler : CommandHandlerBase<CreateSubscriptionCommand, Guid>
    {
        public CreateSubscriptionCommandHandler(IApplicationDbContext context, ILogger<CreateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var activeSubscriptionExists = _context.Subscriptions.Include(x=>x.Payment)
                .Where(s => s.ApplicationUserId == request.ApplicationUserId);

            if (await activeSubscriptionExists.AnyAsync(x=>x.IsActive()))
            {
                throw new ApplicationException("User already has an active subscription");
            }

            if (await activeSubscriptionExists.AnyAsync(x => x.Payment != null && x.Payment.PaymentStatus == PaymentStatus.Pending))
            {
                throw new ApplicationException("User already has started buying the sub");
            }

            var subscription = new Subscription
            {
                PaymentId = request.PaymentId,
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
