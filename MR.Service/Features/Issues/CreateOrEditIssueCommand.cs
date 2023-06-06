namespace MR.Service.Features.Issues;

public class CreateOrEditIssueCommand : IRequest<Guid>
{
    //nullable
    public string CreatedById { get; set; }

    //not null
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    //bazujac na tym statusie ustawiamy widocznosc
    public IssueVisibility IssueVisibility { get; set; } = IssueVisibility.NotVisible;
    public IssueProcess IssueProcess { get; set; } = IssueProcess.InCreation;
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public int RatingValue { get; set; }

    public Guid? IssueId { get; set; }

    public class CreateIssueCommandHandler : CreateOrEditCommandHandlerBase<CreateOrEditIssueCommand, Guid, Issue>
    {
        public CreateIssueCommandHandler(
            IApplicationDbContext context,
            ILogger<CreateOrEditIssueCommand> logger) : base(context, logger)
        {
        }

        protected override async Task<Issue> MakeAsync(CreateOrEditIssueCommand command, CancellationToken cancellationToken)
        {
            Issue issue;

            if (command.IssueId.HasValue)
            {
                issue = await _context.Issues.FirstAsync(x => x.Id == command.IssueId, cancellationToken);
            }
            else
            {
                issue = await base.MakeAsync(command, cancellationToken);
                issue.CreatedById = command.CreatedById;
            }

            issue.IssueVisibilityHistories = new List<IssueVisibilityHistory> { new() { IssueVisibility = command.IssueVisibility } };

            return issue;
        }
    }
}
