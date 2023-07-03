namespace MR.Service.Features.SubscriptionFeatures;

public class RejectSubscriptionCommand : IRequest<bool>
{
    public readonly string ApplicationUserId;
    public RejectSubscriptionCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }
    internal class RejectSubscriptionCommandHandler : PaymentCommandHandlerBase<RejectSubscriptionCommand, bool>
    {
        public RejectSubscriptionCommandHandler(IApplicationDbContext context, ILogger<RejectSubscriptionCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<bool> Handle(RejectSubscriptionCommand request, CancellationToken cancellationToken)
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

            var paymentToReject = userSub.SubscriptionPayments
                .Select(x=>x.Payment)
                .FirstOrDefault(x => x?.PaymentStatus == PaymentStatus.Pending);

            if (paymentToReject == null)
            {
                throw new ApplicationException("User doesn't have any pending sub");
            }
            
            var yesterday = DateTime.UtcNow.AddDays(-1);
            userSub.Begin = yesterday;
            userSub.End = yesterday;

            SetPaymentStatus(paymentToReject, PaymentStatus.Rejected);

            int result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}