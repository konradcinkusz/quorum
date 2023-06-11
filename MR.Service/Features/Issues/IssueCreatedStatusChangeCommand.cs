namespace MR.Service.Features.Issues;

public class IssueCreatedStatusChangeCommand : IRequest<bool>, IIssueCommandData
{
    public Guid IssueId { get; }
    public string CreatedById { get; }

    public IssueCreatedStatusChangeCommand(Guid issueId, string createdById)
    {
        IssueId = issueId;
        CreatedById = createdById;
    }
    public class IssueCreatedStatusChangeCommandHandler : IssueCommandHandlerBase<IssueCreatedStatusChangeCommand, bool>
    {
        public IssueCreatedStatusChangeCommandHandler(MRUserManager MRUserManager, IApplicationDbContext context, ILogger<IssueCreatedStatusChangeCommand> logger) : base(MRUserManager, context, logger)
        {
        }

        public override async Task<bool> Handle(IssueCreatedStatusChangeCommand request, CancellationToken cancellationToken)
        {
            var issue = await CheckBasicConditions(request, cancellationToken);

            if (issue.IssueProcess != IssueProcess.InCreation)
            {
                throw new Exception($"Issue should have status InCreation insted of current: {issue.IssueProcess}");
            }

            issue.IssueProcess = IssueProcess.Created;
            issue.IssueProcessingHistories = new List<IssueProcessingHistory>() { new IssueProcessingHistory() { IssueProcess = IssueProcess.Created } };

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return sum > 0;
        }
    }
}