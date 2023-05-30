namespace MR.Service.Features.SubscriptionFeatures.Queries;

public class GetSubscriptionsThatCouldBeActivateCommand : 
    IRequest<PagedList<Subscription>>
{
    public class GetSubscriptionsThatCouldBeActivateCommandHandler : 
        CommandHandlerBase<GetSubscriptionsThatCouldBeActivateCommand, PagedList<Subscription>>
    {
        public GetSubscriptionsThatCouldBeActivateCommandHandler(IApplicationDbContext context, ILogger<GetSubscriptionsThatCouldBeActivateCommand> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Subscription>> Handle(GetSubscriptionsThatCouldBeActivateCommand request, CancellationToken cancellationToken)
        {
            var query = _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .Where(x => x.Payment.PaymentStatus == PaymentStatus.Accepted && !x.Subscription.Begin.HasValue && !x.Subscription.End.HasValue);

            return await PagedList<Subscription>.CreateAsync(query.Select(x=>x.Subscription).AsQueryable(), new(), cancellationToken);
        }
    }
}
