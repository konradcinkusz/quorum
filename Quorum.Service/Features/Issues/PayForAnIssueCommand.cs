using Quorum.Service.Features.Issues.Base;

namespace Quorum.Service.Features.Issues;

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
            IQuorumUserService users, IApplicationDbContext context, ILogger<PayForAnIssueCommand> logger) : base(users, context, logger)
        {
        }

        public override async Task<bool> Handle(PayForAnIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await CheckBasicConditionsAndReturnIssue(request, cancellationToken);

            if (issue.InitialPayment != null)
            {
                throw new ApplicationException("You have already inititalized payment.");
            }

            if (issue.IssueProcess != IssueProcess.Created)
            {
                throw new ApplicationException("Only issues with Created status can be paid.");
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

            issue.IssueProcess = IssueProcess.PaymentInProgress;
            issue.IssueProcessingHistories = new List<IssueProcessingHistory>() { new IssueProcessingHistory { IssueProcess = IssueProcess.PaymentInitialized }, new IssueProcessingHistory { IssueProcess = IssueProcess.PaymentInProgress } };

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return sum > 0;
        }
    }
}
