namespace MR.Service.Features.Issues;

public class PublishIssueCommand : IRequest<bool>, IIssueCommandData
{
    public string CreatedById { get; }
    public Guid IssueId { get; }
    public PublishIssueCommand(string createdById, Guid issueId)
    {
        CreatedById = createdById;
        IssueId = issueId;
    }

    public class PublishIssueCommandHandler : IssueCommandHandlerBase<PublishIssueCommand, bool>
    {
        public PublishIssueCommandHandler(IMrUserService users, IApplicationDbContext context, ILogger<PublishIssueCommand> logger) : base(users, context, logger)
        {
        }

        public override async Task<bool> Handle(PublishIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await CheckBasicConditionsAndReturnIssue(request, cancellationToken);

            if (issue.InitialPayment == null || issue.InitialPayment.PaymentStatus != PaymentStatus.Completed)
            {
                throw new ApplicationException("You have not completed or have not started the payment process.");
            }

            if (!issue.IsVerifyByAdmin)
            {
                throw new ApplicationException("Your issue hasn't been verified by admin yet.");
            }

            var quarter = await _context.Quarters.FirstOrDefaultAsync(QuarterExtensions.GetCurrentQuarterExpression(), cancellationToken);
            if (quarter == null)
            {
                throw new ApplicationException("No current quarter has been initialized yet. Contact with Admin");
            }

            issue.IssueProcess = IssueProcess.Published;
            issue.IssueProcessingHistories = new List<IssueProcessingHistory>() { new IssueProcessingHistory { IssueProcess = IssueProcess.Publishing }, new IssueProcessingHistory { IssueProcess = IssueProcess.Published } };

            issue.IssueVisibility = IssueVisibility.VisibleForAll;
            issue.IssueVisibilityHistories = new List<IssueVisibilityHistory>() { new IssueVisibilityHistory() { IssueVisibility = IssueVisibility.VisibleForAll } };

            _ = await _context.QuarterIssues.AddAsync(new QuarterIssue { Issue = issue, Quarter = quarter }, cancellationToken);

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
