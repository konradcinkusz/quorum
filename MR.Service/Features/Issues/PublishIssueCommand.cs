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
        public PublishIssueCommandHandler(
            MRUserManager MRUserManager, IApplicationDbContext context, ILogger<PublishIssueCommand> logger) : base(MRUserManager, context, logger)
        {
        }

        public override async Task<bool> Handle(PublishIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await CheckBasicConditions(request, cancellationToken);

            if (issue.InitialPayment == null || issue.InitialPayment.PaymentStatus != PaymentStatus.Completed)
            {
                throw new ApplicationException("You have not completed or have not started the payment process.");
            }

            if (!issue.IsVerifyByAdmin)
            {
                throw new ApplicationException("Your issue hasn't been verified by admin yet.");
            }

            var currentDate = DateTime.UtcNow;
            //get current Quarter
            var quarter = await _context.Quarters.FirstAsync(x => x.Year == currentDate.Year && x.QuarterNumber == currentDate.Month.GetQuarter());
            
            if(quarter == null)
            {
                throw new ApplicationException("No current quarter has been initialized yet. Contact with Admin");
            }
            
            issue.IssueStatus = IssueStatus.VisibleForAll;

            issue.QuarterIssues.Add(new QuarterIssue { IssueId = issue.Id, QuarterId = quarter.Id });

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return sum > 0;
        }
    }
}
