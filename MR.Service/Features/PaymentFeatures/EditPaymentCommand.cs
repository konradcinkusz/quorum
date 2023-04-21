namespace MR.Service.Features.PaymentFeatures;

public class EditPaymentCommand : IRequest<Guid>
{
    public Guid PaymentId { get; set; }
    public string UserEmail { get; set; }
    public string PaymentLink { get; set; }
    public string ClientReferenceId { get; set; }
    public string PaymentIntentId { get; set; }
    public string SessionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string ApplicationUserId { get; set; }
    public decimal PaymentValuePLN { get; set; }

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

            payment.UserEmail = request.UserEmail;
            payment.PaymentLink = request.PaymentLink;
            payment.ClientReferenceId = request.ClientReferenceId;
            payment.PaymentIntentId = request.PaymentIntentId;
            payment.SessionId = request.SessionId;
            payment.PaymentStatus = request.PaymentStatus.ToString();
            payment.PaymentValuePLN = request.PaymentValuePLN;

            await _context.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }

    }
}