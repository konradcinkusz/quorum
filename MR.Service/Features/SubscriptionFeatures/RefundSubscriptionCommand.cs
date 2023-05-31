using MR.Domain.Entities;

namespace MR.Service.Features.SubscriptionFeatures;

public class RefundSubscriptionCommand : IRequest<bool>
{
    public readonly string ApplicationUserId;
    public RefundSubscriptionCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }
    public class RefundSubscriptionCommandHandler : PaymentCommandHandlerBase<RefundSubscriptionCommand, bool>
    {
        public RefundSubscriptionCommandHandler(IApplicationDbContext context, ILogger<RefundSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<bool> Handle(RefundSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var userSub =
                await _context.Subscriptions.Include(x => x.SubscriptionPayments).ThenInclude(x => x.Payment)
                .FirstOrDefaultAsync(s => s.ApplicationUserId == request.ApplicationUserId, cancellationToken);

            if (userSub == null)
            {
                throw new ApplicationException("User don't have subscription record, contact with admin. Subscription had to been added while registering.");
            }

            if (userSub.IsActive())
            {
                throw new ApplicationException("User already has an active subscription");
            }

            var lastPayment = userSub.SubscriptionPayments
                .Select(x => x.Payment).OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault(x => x?.PaymentStatus == PaymentStatus.Completed);

            if (lastPayment == null || !userSub.IsActive())
            {
                throw new ApplicationException("User doesn't have any completed payment or active sub");
            }

            var yesterday = DateTime.UtcNow.AddDays(-1);
            userSub.Begin = yesterday;
            userSub.End = yesterday;

            SetPaymentStatus(lastPayment, PaymentStatus.Refunded);

            int result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}
