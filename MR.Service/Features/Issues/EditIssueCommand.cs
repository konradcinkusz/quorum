namespace MR.Service.Features.Issues;

public class EditIssueCommand : IRequest<int>
{
    public string? Title { get; set; }
    public string? Question { get; set; }
    public bool? IsVerifyByAdmin { get; set; } = false;
    public IssueVisibility? IssueVisibility { get; set; }
    public IssueProcess? IssueProcess { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public int? RatingValue { get; set; }
    private readonly Guid _id;

    public EditIssueCommand(Guid id)
    {
        _id = id;
    }

    public class EditIssueCommandHandler : CommandHandlerBase<EditIssueCommand, int>
    {
        public EditIssueCommandHandler(IApplicationDbContext context, ILogger<EditIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<int> Handle(EditIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueVisibilityHistories)
                .Include(x => x.IssueProcessingHistories)
                .Include(x => x.CreatedBy)
                .FirstAsync(x => x.Id == request._id, cancellationToken);

            if (issue != null)
            {
                // Update the properties with the provided values
                issue.Title = request.Title ?? issue.Title;
                issue.Question = request.Question ?? issue.Question;
                issue.IsVerifyByAdmin = request.IsVerifyByAdmin ?? issue.IsVerifyByAdmin;
                issue.IssueVisibility = request.IssueVisibility ?? issue.IssueVisibility;
                issue.IssueProcess = request.IssueProcess ?? issue.IssueProcess;
                issue.Icon = request.Icon ?? issue.Icon;
                issue.BackgroundColor = request.BackgroundColor ?? issue.BackgroundColor;
                issue.RatingValue = request.RatingValue ?? issue.RatingValue;
            }

            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
