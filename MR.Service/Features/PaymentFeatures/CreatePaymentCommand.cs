namespace MR.Service.Features.PaymentFeatures;
public class CreatePaymentCommand : IPaymentBaseFeature, IRequest<Guid>
{
    public PaymentStatus PaymentStatus { get; set; }
    public string ApplicationUserId { get; set; }
    public decimal PaymentValuePLN { get; set; }
    public string PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    public string ReferenceNumber { get; set; }// a reference number associated with the payment (e.g. transaction ID)
    public class CreatePaymentCommandHandler : CommandHandlerBase<CreatePaymentCommand, Guid>
    {
        public CreatePaymentCommandHandler(IApplicationDbContext context, ILogger<CreatePaymentCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = MakePayment(request);

            await _context.Payments.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }

        private Payment MakePayment(CreatePaymentCommand request)
        {
            return new Payment
            {
                PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber,
                PaymentStatus = request.PaymentStatus,
                ApplicationUserId = request.ApplicationUserId,
                PaymentValuePLN = request.PaymentValuePLN,
                PaymentStatusHistories = new List<PaymentStatusHistory> {
                    new PaymentStatusHistory {
                        PaymentStatus = request.PaymentStatus
                    }
                }
            };
        }
    }
}