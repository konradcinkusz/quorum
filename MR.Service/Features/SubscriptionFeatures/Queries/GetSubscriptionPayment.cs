namespace MR.Service.Features.SubscriptionFeatures.Queries;

public class GetSubscriptionPayment : IRequest<Payment?>
{
    public string? ApplicationUserId { get; set; }

    public class GetSubscriptionPaymentHandler : CommandHandlerBase<GetSubscriptionPayment, Payment?>
    {
        public GetSubscriptionPaymentHandler(IApplicationDbContext context, ILogger<GetSubscriptionPayment> logger) : base(context, logger)
        {
        }

        public override async Task<Payment?> Handle(GetSubscriptionPayment request, CancellationToken cancellationToken)
        {
            var query = await _context.Subscriptions
                .Where(s => s.ApplicationUserId == request.ApplicationUserId)
                .SelectMany(s => s.SubscriptionPayments)
                .Include(sp => sp.Payment.PaymentStatusHistories)
                .Select(sp => sp.Payment)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            return query;
        }
    }
}
