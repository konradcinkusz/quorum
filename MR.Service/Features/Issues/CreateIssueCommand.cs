namespace MR.Service.Features.Issues;

public class CreateIssueCommand : IRequest<Guid>
{
    //nullable
    public string ApplicationUserId { get; set; }

    //not null
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    //bazujac na tym statusie ustawiamy widocznosc
    public IssueStatus IssueStatus { get; set; } = IssueStatus.NotVisible;
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public int RatingValue { get; set; }

    public class CreateIssueCommandHandler : CreateCommandHandlerBase<CreateIssueCommand, Guid, Issue>
    {
        public CreateIssueCommandHandler(
            IApplicationDbContext context,
            ILogger<CreateIssueCommand> logger) : base(context, logger)
        {
        }

        protected override async Task<Issue> MakeAsync(CreateIssueCommand command, CancellationToken cancellationToken)
        {
            var issue = await base.MakeAsync(command, cancellationToken);
            issue.CreatedById = command.ApplicationUserId;

            issue.IssueStatusHistories = new List<IssueStatusHistory> { new() { IssueStatus = command.IssueStatus } };

            return issue;
        }
    }
}
