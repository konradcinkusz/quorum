namespace MR.Service.Features.Issues;

public class CreateIssueCommand : IRequest<Guid>
{
    public required string CreatedById { get; set; }
    public required string Title { get; set; }
    public required string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    public IssueVisibility? IssueVisibility { get; set; }
    public IssueProcess? IssueProcess { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }

    internal class CreateIssueCommandHandler : CreateCommandHandlerBase<CreateIssueCommand, Guid, Issue>
    {
        public CreateIssueCommandHandler(IApplicationDbContext context, ILogger<CreateIssueCommand> logger) : base(context, logger)
        {
        }

        protected override async Task<Issue> MakeAsync(CreateIssueCommand command, CancellationToken cancellationToken)
        {
            var issue = await base.MakeAsync(command, cancellationToken);
            issue.CreatedById = command.CreatedById;

            if (command.IssueVisibility.HasValue && issue.IssueVisibility != command.IssueVisibility.Value)
            {
                issue.IssueVisibility = command.IssueVisibility.Value;
                issue.IssueVisibilityHistories = new List<IssueVisibilityHistory> { new() { IssueVisibility = command.IssueVisibility.Value } };
            }

            if (command.IssueProcess.HasValue && issue.IssueProcess != command.IssueProcess.Value)
            {
                issue.IssueProcess = command.IssueProcess.Value;
                issue.IssueProcessingHistories = new List<IssueProcessingHistory> { new() { IssueProcess = command.IssueProcess.Value } };
            }

            return issue;
        }
    }
}
