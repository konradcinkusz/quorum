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
    public IssueStatus IssueStatus { get; set; } = IssueStatus.NotVisible;
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public int RatingValue { get; set; }

    public Guid? IssueId { get; set; }

    public class CreateIssueCommandHandler : CreateOrEditCommandHandlerBase<CreateOrEditIssueCommand, Guid, Issue>
    {
        private readonly MRUserManager _MRUserManager;
        public CreateIssueCommandHandler(
            MRUserManager MRUserManager,
            IApplicationDbContext context,
            ILogger<CreateOrEditIssueCommand> logger) : base(context, logger)
        {
            _MRUserManager = MRUserManager;
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

            issue.IssueStatusHistories = new List<IssueStatusHistory> { new() { IssueStatus = command.IssueStatus } };

            return issue;
        }
    }
}
