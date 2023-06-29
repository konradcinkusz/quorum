namespace MR.Service.Features.Issues;

public class IssueCreatedStatusChangeCommand : IRequest<bool>
{
    public Guid IssueId { get; }

    public IssueCreatedStatusChangeCommand(Guid issueId)
    {
        IssueId = issueId;
    }
    internal class IssueCreatedStatusChangeCommandHandler : CommandHandlerBase<IssueCreatedStatusChangeCommand, bool>
    {
        public IssueCreatedStatusChangeCommandHandler(IApplicationDbContext context, ILogger<IssueCreatedStatusChangeCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(IssueCreatedStatusChangeCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues.FirstAsync(x => x.Id == request.IssueId, cancellationToken);

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