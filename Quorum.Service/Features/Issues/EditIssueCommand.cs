namespace Quorum.Service.Features.Issues;

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
    private readonly IssueOwnerScope _scope;

    /// <param name="scope">
    /// Which issues this caller may edit. Use <see cref="IssueOwnerScope.OwnedBy"/> for a
    /// user-facing route and <see cref="IssueOwnerScope.Administrator"/> only from a route
    /// already gated behind the admin policy.
    /// </param>
    public EditIssueCommand(Guid id, IssueOwnerScope scope)
    {
        _id = id;
        _scope = scope;
    }

    public class EditIssueCommandHandler : CommandHandlerBase<EditIssueCommand, int>
    {
        public EditIssueCommandHandler(IApplicationDbContext context, ILogger<EditIssueCommand> logger) : base(context, logger)
        {
        }

        public override async Task<int> Handle(EditIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues
                .RestrictToOwner(request._scope)
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueVisibilityHistories)
                .Include(x => x.IssueProcessingHistories)
                .FirstOrDefaultAsync(x => x.Id == request._id, cancellationToken);

            // Deliberately the same result whether the issue does not exist or belongs to
            // someone else: a caller must not be able to probe for other users' issue ids.
            if (issue == null)
            {
                throw new NotFoundException(nameof(Issue), request._id);
            }

            // Update the properties with the provided values
            issue.Title = request.Title ?? issue.Title;
            issue.Question = request.Question ?? issue.Question;
            issue.IsVerifyByAdmin = request.IsVerifyByAdmin ?? issue.IsVerifyByAdmin;
            issue.IssueVisibility = request.IssueVisibility ?? issue.IssueVisibility;
            issue.IssueProcess = request.IssueProcess ?? issue.IssueProcess;
            issue.Icon = request.Icon ?? issue.Icon;
            issue.BackgroundColor = request.BackgroundColor ?? issue.BackgroundColor;
            issue.RatingValue = request.RatingValue ?? issue.RatingValue;

            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
