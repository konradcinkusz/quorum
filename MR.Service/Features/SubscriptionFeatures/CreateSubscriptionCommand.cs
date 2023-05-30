namespace MR.Service.Features.SubscriptionFeatures;

public class CreateSubscriptionCommand : IRequest<bool>
{
    public readonly string ApplicationUserId;
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public CreateSubscriptionCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }
    public class CreateSubscriptionCommandHandler : CommandHandlerBase<CreateSubscriptionCommand, bool>
    {
        public CreateSubscriptionCommandHandler(IApplicationDbContext context, ILogger<CreateSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<bool> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subscritpion = await _context.Subscriptions.Where(x => x.ApplicationUserId == request.ApplicationUserId).FirstOrDefaultAsync(cancellationToken);
            if (subscritpion != null)
            {
                subscritpion.Begin = request.Begin;
                subscritpion.End = request.End;
            }
            else
            {
                var subscription = new Subscription
                {
                    ApplicationUserId = request.ApplicationUserId,
                    Begin = request.Begin,
                    End = request.End
                };

                await _context.Subscriptions.AddAsync(subscription, cancellationToken);
            }

            var result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}
