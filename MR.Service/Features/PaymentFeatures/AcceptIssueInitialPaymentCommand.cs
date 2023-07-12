namespace MR.Service.Features.PaymentFeatures;

public class AcceptIssueInitialPaymentCommand : IRequest<bool>
{
    public readonly Guid PaymentId;
    public AcceptIssueInitialPaymentCommand(Guid paymentId)
    {
        PaymentId = paymentId;
    }

    internal class AcceptIssueInitialPaymentCommandHandler : PaymentCommandHandlerBase<AcceptIssueInitialPaymentCommand, bool>
    {
        public AcceptIssueInitialPaymentCommandHandler(IApplicationDbContext context, ILogger<AcceptIssueInitialPaymentCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(AcceptIssueInitialPaymentCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues
                .Include(x=>x.InitialPayment)
                .FirstOrDefaultAsync(x => x.InitialPayment != null && x.InitialPayment.Id == request.PaymentId, cancellationToken);
            
            if (issue == null)
            {
                throw new ApplicationException("There is no issue with that payment relation.");
            }

            issue.IssueProcessingHistories = new List<IssueProcessingHistory>();

            var payment = issue.InitialPayment;
            if (payment == null)
            {
                throw new InvalidOperationException($"There are no payment with provided Guid: {request.PaymentId}");
            }

            if (payment.PaymentStatus != PaymentStatus.Pending)
            {
                throw new ApplicationException($"I can only accept payment with pending status, but there are no such a payment now Guid: {request.PaymentId}");
            }

            issue.IssueProcessingHistories.Add(new() { IssueProcess = IssueProcess.PaymentInProgress });
            SetPaymentStatus(payment, PaymentStatus.Accepted);
            SetPaymentStatus(payment, PaymentStatus.Completed);
            issue.IssueProcessingHistories.Add(new() { IssueProcess = IssueProcess.PaymentCompleted });
            issue.IssueProcessingHistories.Add(new() { IssueProcess = IssueProcess.InAdminVerification });

            issue.IssueProcess = IssueProcess.InAdminVerification;

            _ = await _context.IssueRatingHistories.AddAsync(new IssueRatingHistory() { Issue = issue, Value = payment.PaymentValuePLN, Action= RatingAction.InitialPayment, RelatedObject = $"PaymentId: {request.PaymentId}" }, cancellationToken);

            int result = await _context.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}
