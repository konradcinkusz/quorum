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

    public class CreateIssueCommandHandler : CreateCommandHandlerBase<CreateIssueCommand, Guid, Issue>
    {
        public CreateIssueCommandHandler(
            IApplicationDbContext context,
            ILogger<CreateIssueCommand> logger) : base(context, logger)
        {
        }

        protected override Task<Issue> MakeAsync(CreateIssueCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Issue
            {
                CreatedById = command.ApplicationUserId,
                Title = command.Title,
                Question = command.Question,
                IsVerifyByAdmin = command.IsVerifyByAdmin,
                IssueStatus = command.IssueStatus
            });
        }
    }
}
