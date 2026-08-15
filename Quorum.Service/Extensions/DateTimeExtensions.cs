namespace Quorum.Service.Extensions;

public static class DateTimeExtensions
{
    public static int GetQuarter(this int month)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Invalid month number. Month number should be between 1 and 12.");
        }

        return (month - 1) / 3 + 1;
    }
}
