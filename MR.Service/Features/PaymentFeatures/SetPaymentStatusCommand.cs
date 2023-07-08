namespace MR.Service.Features.PaymentFeatures;

public class SetPaymentStatusCommand : IRequest<bool>
{
    private readonly Guid _paymentId;
    private readonly PaymentStatus _paymentStatus;
    public SetPaymentStatusCommand(Guid paymentId, PaymentStatus paymentStatus)
    {
        _paymentId = paymentId;
        _paymentStatus = paymentStatus;
    }

    internal class SetPaymentStatusCommandHandler : PaymentCommandHandlerBase<SetPaymentStatusCommand, bool>
    {
        public SetPaymentStatusCommandHandler(IApplicationDbContext context, ILogger<SetPaymentStatusCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(SetPaymentStatusCommand request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(x => x.Id == request._paymentId);
            if (payment != null)
            {
                SetPaymentStatus(payment, request._paymentStatus);
            }
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
