namespace MR.Service.Features.PaymentFeatures;

public class CreatePaymentCommand : IRequest<Guid>
{
    public string UserEmail { get; set; }
    public string PaymentLink { get; set; }
    public string ClientReferenceId { get; set; }
    public string PaymentIntentId { get; set; }
    public string SessionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string ApplicationUserId { get; set; }
    public decimal PaymentValuePLN { get; set; }

    public class CreatePaymentCommandHandler : CommandHandlerBase<CreatePaymentCommand, Guid>
    {
        public CreatePaymentCommandHandler(IApplicationDbContext context, ILogger<CreatePaymentCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = new Payment
            {
                UserEmail = request.UserEmail,
                PaymentLink = request.PaymentLink,
                ClientReferenceId = request.ClientReferenceId,
                PaymentIntentId = request.PaymentIntentId,
                SessionId = request.SessionId,
                PaymentStatus = request.PaymentStatus.ToString(),
                ApplicationUserId = request.ApplicationUserId,
                PaymentValuePLN = request.PaymentValuePLN,
                PaymentStatusHistories = new List<PaymentStatusHistory> {
                    new PaymentStatusHistory {
                        PaymentStatus = request.PaymentStatus
                    }
                }
            };

            await _context.Payments.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }

    }
}