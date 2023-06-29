namespace MR.Service.Features.Issues;

public class VerifyByAdminCommand : IRequest<bool>
{
    public Guid IssueId { get; }
    public bool Confirmed { get; }

    public VerifyByAdminCommand(Guid issueId, bool confirmed)
    {
        IssueId = issueId;
        Confirmed = confirmed;
    }

    internal class VerifyByAdminCommandHandler : CommandHandlerBase<VerifyByAdminCommand, bool>
    {
        public VerifyByAdminCommandHandler(IApplicationDbContext context, ILogger<VerifyByAdminCommand> logger) : base(context, logger)
        {
        }

        public override async Task<bool> Handle(VerifyByAdminCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues.FirstAsync(x => x.Id == request.IssueId, cancellationToken);
            if (issue != null && issue.IssueProcess == IssueProcess.InAdminVerification)
            {
                issue.IsVerifyByAdmin = request.Confirmed;
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