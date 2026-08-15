namespace Quorum.Service.Features.PaymentFeatures;

public class EditPaymentCommand : IRequest<int>
{
    private readonly Guid _paymentId;
    public PaymentStatus? PaymentStatus { get; set; }
    public string? ApplicationUserId { get; set; }
    public decimal? PaymentValuePLN { get; set; }
    public string? PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    public string? ReferenceNumber { get; set; }// a reference number associated with the payment (e.g. transaction ID)

    public EditPaymentCommand(Guid paymentId)
    {
        _paymentId = paymentId;
    }

    internal class EditPaymentCommandHandler : CommandHandlerBase<EditPaymentCommand, int>
    {
        public EditPaymentCommandHandler(IApplicationDbContext context, ILogger<EditPaymentCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<int> Handle(EditPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentStatusHistories).
                FirstOrDefaultAsync(p => p.Id == request._paymentId, cancellationToken);

            if (payment != null)
            {
                if (request.PaymentStatus.HasValue && payment.PaymentStatus != request.PaymentStatus)
                {
                    payment.PaymentStatusHistories.Add(new PaymentStatusHistory { PaymentStatus = request.PaymentStatus.Value });
                }

                payment.ReferenceNumber = request.ReferenceNumber ?? payment.ReferenceNumber;
                payment.PaymentMethod = request.PaymentMethod ?? payment.PaymentMethod;
                payment.PaymentValuePLN = request.PaymentValuePLN ?? payment.PaymentValuePLN;
                payment.PaymentStatus = request.PaymentStatus ?? payment.PaymentStatus;
            }

            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
