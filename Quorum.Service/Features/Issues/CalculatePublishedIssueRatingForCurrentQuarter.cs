namespace Quorum.Service.Features.Issues;

public class CalculatePublishedIssueRatingForCurrentQuarter : IRequest<PagedList<Issue>>
{
    internal class CalculatePublishedIssueRatingForCurrentQuarterHandler : CommandHandlerBase<CalculatePublishedIssueRatingForCurrentQuarter, PagedList<Issue>>
    {
        public CalculatePublishedIssueRatingForCurrentQuarterHandler(IApplicationDbContext context, ILogger<CalculatePublishedIssueRatingForCurrentQuarter> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(CalculatePublishedIssueRatingForCurrentQuarter request, CancellationToken cancellationToken)
        {
            var query = _context.Issues
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueRatingHistories)
                .Where(x =>
                        !x.IsDeleted &&
                        x.IsVerifyByAdmin &&
                        x.InitialPayment != null &&
                        x.IssueProcess == IssueProcess.Published &&
                        x.IssueVisibility == IssueVisibility.VisibleForAll);

            var currentDate = DateTime.UtcNow;
            var currentYear = currentDate.Year;
            var currentQuarter = (currentDate.Month - 1) / 3 + 1;
            query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.Year == currentYear));
            query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.QuarterNumber == currentQuarter));

            var issues = await query.ToListAsync(cancellationToken);
            var resultIssues = new List<Issue>();
            foreach (var issue in issues)
            {
                var ratingValue = issue.IssueRatingHistories.Sum(x => x.Value);
                if (issue.RatingValue != ratingValue)
                {
                    issue.RatingValue = ratingValue;
                    resultIssues.Add(issue);
                }
            }

            var sum = await _context.SaveChangesAsync(cancellationToken);

            return PagedList<Issue>.Create(resultIssues, new());
        }
    }
}
