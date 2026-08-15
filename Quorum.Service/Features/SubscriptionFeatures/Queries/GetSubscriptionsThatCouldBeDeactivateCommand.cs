using Quorum.Service.ViewModels;

namespace Quorum.Service.Features.SubscriptionFeatures.Queries;

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
                        .Include(x => x.SubscriptionPayments)
                            .ThenInclude(x => x.Payment)
                        .Where(x => x.SubscriptionPayments.Any(sp =>
                            sp.Payment.PaymentStatus == PaymentStatus.Completed &&
                            sp.Subscription.End >= currentDate))
                        .Select(x => new Subscription
                        {
                            ApplicationUserId = x.ApplicationUserId,
                            Begin = x.Begin,
                            End = x.End,
                            CreatedAt = x.CreatedAt,
                            SubscriptionPayments = x.SubscriptionPayments
                                .OrderByDescending(sp => sp.Payment.CreatedAt)
                                .ToList()
                        });

            var pagedList = await PagedList<Subscription>.CreateAsync(query, new(), cancellationToken);

            await _context.PopulateUserEmailsAsync(
                pagedList, x => x.ApplicationUserId, (x, email) => x.ApplicationUserEmail = email, cancellationToken);

            return pagedList;
        }
    }
}