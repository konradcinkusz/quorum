namespace MR.Service.Features.SubscriptionFeatures.Queries;

public class GetSubscriptionsThatCouldBeDeactivateCommand :
    IRequest<PagedList<Subscription>>
{
    public class GetSubscriptionsThatCouldBeDeactivateCommandHandler :
        CommandHandlerBase<GetSubscriptionsThatCouldBeDeactivateCommand, PagedList<Subscription>>
    {
        public GetSubscriptionsThatCouldBeDeactivateCommandHandler(IApplicationDbContext context, ILogger<GetSubscriptionsThatCouldBeDeactivateCommand> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Subscription>> Handle(GetSubscriptionsThatCouldBeDeactivateCommand request, CancellationToken cancellationToken)
        {
            var currentDate = DateTime.UtcNow;

            var query = _context.SubscriptionPayment
                .Include(x => x.Subscription)
                .Include(x => x.Payment)
                .Where(x => x.Payment.PaymentStatus == PaymentStatus.Accepted
                    && x.Subscription.End >= currentDate);

            return await PagedList<Subscription>.CreateAsync(query.Select(x => x.Subscription).AsQueryable(), new(), cancellationToken);
        }
    }
}