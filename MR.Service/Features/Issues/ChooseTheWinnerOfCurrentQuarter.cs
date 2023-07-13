namespace MR.Service.Features.Issues;

public class ChooseTheWinnerOfCurrentQuarter : IRequest<Issue>
{
    internal class ChooseTheWinnerOfCurrentQuarterHandler : CommandHandlerBase<ChooseTheWinnerOfCurrentQuarter, Issue>
    {
        public ChooseTheWinnerOfCurrentQuarterHandler(IApplicationDbContext context, ILogger<ChooseTheWinnerOfCurrentQuarter> logger) : base(context, logger)
        {
        }

        public override async Task<Issue> Handle(ChooseTheWinnerOfCurrentQuarter request, CancellationToken cancellationToken)
        {
            var currentDate = DateTime.UtcNow;
            var currentYear = currentDate.Year;
            var currentQuarter = (currentDate.Month - 1) / 3 + 1;

            var issues = await _context.Issues
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueRatingHistories)
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsVerifyByAdmin &&
                    x.InitialPayment != null &&
                    x.IssueProcess == IssueProcess.Published &&
                    x.IssueVisibility == IssueVisibility.VisibleForAll &&
                    x.QuarterIssues.Any(y => y.Quarter.Year == currentYear && y.Quarter.QuarterNumber == currentQuarter))
                .OrderByDescending(x => x.RatingValue)
                .ToListAsync(cancellationToken);

            var issueWinner = issues.FirstOrDefault();

            if (issueWinner == null)
            {
                throw new Exception($"There are no issue to choose the winner");
            }

            var currentQuarterIssues = await _context.QuarterIssues.Include(x => x.Issue).Include(x => x.Quarter).Where(y => y.Quarter.Year == currentYear && y.Quarter.QuarterNumber == currentQuarter).ToListAsync(cancellationToken);
            //Ustawienie wygranego
            foreach (var currentQuarterIssue in currentQuarterIssues)
            {
                currentQuarterIssue.QuarterWinner = currentQuarterIssue.IssueId == issueWinner.Id;
            }

            //Zamknięcie kwartału
            var currentQuarterResult = await _context.Quarters.SingleOrDefaultAsync(QuarterExtensions.GetCurrentQuarterExpression(), cancellationToken);
            currentQuarterResult.QuarterResolved = true;

            //zmiana kroku procesu i widoczności dla aktualnych kwestii
            foreach (var issue in issues)
            {
                issue.IssueProcess = IssueProcess.EndedInCurrentQuarter;
                issue.IssueProcessingHistories = new List<IssueProcessingHistory>() { new IssueProcessingHistory { IssueProcess = IssueProcess.EndedInCurrentQuarter } };
            }

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return issueWinner;
        }
    }
}
