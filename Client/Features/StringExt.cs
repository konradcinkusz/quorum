namespace MR.Client.Features;

public static class StringExt
{
    //https://stackoverflow.com/a/2776689/4510954
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value.Substring(0, maxLength) + truncationSuffix
            : value;
    }
}