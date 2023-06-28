namespace MR.Service.Features.Issues;

public class EditIssueCommand : IRequest<Guid>
{
    public string? Title { get; set; }
    public string? Question { get; set; }
    public bool? IsVerifyByAdmin { get; set; } = false;
    public IssueVisibility? IssueVisibility { get; set; }
    public IssueProcess? IssueProcess { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public int RatingValue { get; set; }
    public Guid Id { get; }
    public EditIssueCommand(Guid id)
    {
        Id = id;
    }
    internal class EditIssueCommandHandler : EditCommandHandlerBase<CreateIssueCommand, Guid, Issue>
    {
        public EditIssueCommandHandler(IApplicationDbContext context, ILogger<CreateIssueCommand> logger) : base(context, logger)
        {
        }
    }
}
