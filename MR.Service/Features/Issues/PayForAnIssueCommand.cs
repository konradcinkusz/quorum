namespace MR.Service.Features.Issues;

public class PayForAnIssueCommand : IRequest<bool>, IIssueCommandData
{
    public string CreatedById { get; }
    public Guid IssueId { get; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? PaymentValue { get; set; }
    public PayForAnIssueCommand(string createdById, Guid issueId)
    {
        CreatedById = createdById;
        IssueId = issueId;
    }
    public class PayForAnIssueCommandHandler : IssueCommandHandlerBase<PayForAnIssueCommand, bool>
    {
        public PayForAnIssueCommandHandler(
            MRUserManager MRUserManager, IApplicationDbContext context, ILogger<PayForAnIssueCommand> logger) : base(MRUserManager, context, logger)
        {
        }

        public override async Task<bool> Handle(PayForAnIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await CheckBasicConditions(request, cancellationToken);

            if (issue.InitialPayment != null)
            {
                throw new ApplicationException("You have already inititalized payment.");
            }

            var paymentValue = request.PaymentValue.HasValue ? request.PaymentValue.Value : 0;

            issue.InitialPayment = new Payment
            {
                ApplicationUserId = request.CreatedById,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = PaymentStatus.New,
                PaymentValuePLN = paymentValue,
                ReferenceNumber = request.ReferenceNumber,
                PaymentStatusHistories = new List<PaymentStatusHistory> { new PaymentStatusHistory { PaymentStatus = PaymentStatus.New } }
            };

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return sum > 0;
        }
    }
}