namespace MR.Service.Features.Issues;

public class VerifyByAdminCommand : IRequest<bool>
{
    private readonly Guid _issueId;
    private readonly bool _confirmed;

    public VerifyByAdminCommand(Guid issueId, bool confirmed)
    {
        _issueId = issueId;
        _confirmed = confirmed;
    }

    internal class VerifyByAdminCommandHandler : CommandHandlerBase<VerifyByAdminCommand, bool>
    {
        public VerifyByAdminCommandHandler(IApplicationDbContext context, ILogger<VerifyByAdminCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(VerifyByAdminCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues.FirstAsync(x => x.Id == request._issueId, cancellationToken);
            if (issue != null && issue.IssueProcess == IssueProcess.InAdminVerification)
            {
                issue.IsVerifyByAdmin = request._confirmed;
                issue.IssueProcessingHistories = new List<IssueProcessingHistory>
                {
                    new IssueProcessingHistory() { IssueProcess = IssueProcess.AdminVerificationPassed }
                };
                issue.IssueProcess = IssueProcess.AdminVerificationPassed;
            }
            var sum = await _context.SaveChangesAsync(cancellationToken);
            return sum > 0;
        }
    }
}
