namespace MR.Service.Features.PaymentFeatures;

public class AcceptPaymentCommand : IRequest<bool>
{
    public readonly Guid PaymentId;
    public AcceptPaymentCommand(Guid paymentId)
    {
        PaymentId = paymentId;
    }
    internal class AcceptPaymentCommandHandler : PaymentCommandHandlerBase<AcceptPaymentCommand, bool>
    {
        public AcceptPaymentCommandHandler(IApplicationDbContext context, ILogger<AcceptPaymentCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(AcceptPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(x => x.Id == request.PaymentId);

            if (payment == null)
            {
                throw new InvalidOperationException($"There are no payment with provided Guid: {request.PaymentId}");
            }

            if(payment.PaymentStatus != PaymentStatus.Pending)
            {
                throw new ApplicationException($"I can only accept payment with pending status, but there are no such a payment now Guid: {request.PaymentId}");
            }

            SetPaymentStatus(payment, PaymentStatus.Accepted);

            int result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}
