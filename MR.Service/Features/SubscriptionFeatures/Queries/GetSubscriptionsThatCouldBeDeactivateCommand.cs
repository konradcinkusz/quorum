using MR.Service.ViewModels;

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

            var query = _context.Subscriptions
                        .Include(x => x.ApplicationUser)
                        .Include(x => x.SubscriptionPayments)
                            .ThenInclude(x => x.Payment)
                        .Where(x => x.SubscriptionPayments.Any(sp =>
                            sp.Payment.PaymentStatus == PaymentStatus.Completed &&
                            sp.Subscription.End >= currentDate))
                        .Select(x => new Subscription
                        {
                            ApplicationUserId = x.ApplicationUserId,
                            ApplicationUser = x.ApplicationUser,
                            Begin = x.Begin,
                            End = x.End,
                            CreatedAt = x.CreatedAt,
                            SubscriptionPayments = x.SubscriptionPayments
                                .OrderByDescending(sp => sp.Payment.CreatedAt)
                                .ToList()
                        });

            return await PagedList<Subscription>.CreateAsync(query, new(), cancellationToken);
        }
    }
}