namespace MR.Service.Features.PaymentFeatures;

public class CreatePaymentCommand : IRequest<Guid>
{
    public PaymentStatus? PaymentStatus { get; set; }
    public string? ApplicationUserId { get; set; }
    public decimal? PaymentValuePLN { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public class CreatePaymentCommandHandler : CreateOrEditCommandHandlerBase<CreatePaymentCommand, Guid, Payment>
    {
        public CreatePaymentCommandHandler(IApplicationDbContext context, ILogger<CreatePaymentCommand> logger) : base(context, logger)
        {
        }

        protected override Task<Payment> MakeAsync(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            var payment = new Payment
            {
                PaymentMethod = command.PaymentMethod,
                ReferenceNumber = command.ReferenceNumber,
                ApplicationUserId = command.ApplicationUserId,
                PaymentValuePLN = command.PaymentValuePLN.HasValue ? command.PaymentValuePLN.Value : 0,
                PaymentStatusHistories = new List<PaymentStatusHistory>()
            };

            PaymentStatus pStatus = Domain.Enums.PaymentStatus.None;

            if (command.PaymentStatus.HasValue)
            {
                pStatus = command.PaymentStatus.Value;
            }

            payment.PaymentStatus = pStatus;
            payment.PaymentStatusHistories.Add(new PaymentStatusHistory
            {
                PaymentStatus = pStatus
            });

            return Task.FromResult(payment);
        }
    }
}