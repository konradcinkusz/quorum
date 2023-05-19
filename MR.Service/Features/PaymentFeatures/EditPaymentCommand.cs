namespace MR.Service.Features.PaymentFeatures;

public class EditPaymentCommand : IRequest<Guid>
{
    public Guid PaymentId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string ApplicationUserId { get; set; }
    public decimal PaymentValuePLN { get; set; }
    public string PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    public string ReferenceNumber { get; set; }// a reference number associated with the payment (e.g. transaction ID)

    public class EditPaymentCommandHandler : CommandHandlerBase<EditPaymentCommand, Guid>
    {
        public EditPaymentCommandHandler(IApplicationDbContext context, ILogger<EditPaymentCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<Guid> Handle(EditPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments.Include(p => p.PaymentStatusHistories).FirstOrDefaultAsync(p => p.Id == request.PaymentId);

            if (payment == null)
            {
                _logger.LogError("Payment with ID {PaymentId} was not found", request.PaymentId);
                throw new NotFoundException(nameof(Payment), request.PaymentId);
            }

            if (payment.PaymentStatus != request.PaymentStatus)
            {
                payment.PaymentStatusHistories.Add(new PaymentStatusHistory { PaymentStatus = request.PaymentStatus });
            }

            payment.ReferenceNumber = request.ReferenceNumber;
            payment.PaymentMethod = request.PaymentMethod;
            payment.PaymentStatus = request.PaymentStatus;
            payment.PaymentValuePLN = request.PaymentValuePLN;

            await _context.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }

    }
}