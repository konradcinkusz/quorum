namespace MR.Service.Features.SubscriptionFeatures;

public class CreateSubscriptionCommand : ISubscriptionBaseCommand, IRequest<bool>
{
    public string ApplicationUserId { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public class CreateSubscriptionCommandHandler : CommandHandlerBase<CreateSubscriptionCommand, bool>
    {
        public CreateSubscriptionCommandHandler(IApplicationDbContext context, ILogger<CreateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<bool> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subscription = new Subscription
            {
                ApplicationUserId = request.ApplicationUserId,
                Begin = request.Begin,
                End = request.End
            };

            await _context.Subscriptions.AddAsync(subscription, cancellationToken);
            var result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}
