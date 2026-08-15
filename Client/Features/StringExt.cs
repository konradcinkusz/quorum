namespace Quorum.Client.Features;

public static class StringExt
{
    //https://stackoverflow.com/a/2776689/4510954
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value.Substring(0, maxLength) + truncationSuffix
            : value;
    }

    public static string ConvertToQ(this int? number)
    {
        string[] qNumerals = { "", "Q1", "Q2", "Q3", "Q4" };

        if (!number.HasValue || number < 1 || number > 4)
        {
            throw new ArgumentException("Invalid number. Conversion to Q numeral is supported only for 1, 2, 3, and 4.");
        }

        return qNumerals[number.Value];
    }
}
