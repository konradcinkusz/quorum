namespace Quorum.Service.Extensions;

internal static class QuarterExtensions
{
    public static Quarter? GetCurrentQuarter(this IQueryable<Quarter> query)
    {
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;
        var currentQuarter = (currentDate.Month - 1) / 3 + 1;

        return query.FirstOrDefault(p => p.Year == currentYear && p.QuarterNumber == currentQuarter);
    }

    public static IQueryable<Quarter> GetCurrentAndFutureQuarters(this IQueryable<Quarter> query)
    {
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;
        var currentMonth = (currentDate.Month - 1) / 3 + 1;

        return query.Where(p => (p.Year > currentYear) || (p.Year == currentYear && p.QuarterNumber >= currentMonth));
    }

    public static Func<QuarterIssue, bool> GetCurrentQuarterIssueExpression()
    {
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;
        var currentQuarter = (currentDate.Month - 1) / 3 + 1;
        return x => x.Quarter.Year == currentYear && x.Quarter.QuarterNumber == currentQuarter;
    }

    public static Expression<Func<Quarter, bool>> GetCurrentQuarterExpression()
    {
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;
        var currentQuarter = (currentDate.Month - 1) / 3 + 1;
        return x => x.Year == currentYear && x.QuarterNumber == currentQuarter;
    }

    public static bool CheckCurrentQuarter(Quarter quarter)
    {
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;
        var currentQuarter = (currentDate.Month - 1) / 3 + 1;
        return quarter.Year == currentYear && quarter.QuarterNumber == currentQuarter;
    }
}