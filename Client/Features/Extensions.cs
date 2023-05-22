namespace MR.Client.Features;

public static class Extensions
{
    public static List<string> ExtractStringList(string input)
    {
        // Check if input is null or empty
        if (string.IsNullOrEmpty(input))
        {
            return new List<string>();
        }

        // Remove opening and closing brackets from input
        input = input.TrimStart('[').TrimEnd(']');

        // Split input by commas and remove any surrounding whitespace and quotes
        string[] values = input.Split(',')
            .Select(x => x.Trim().TrimStart('"').TrimEnd('"'))
            .ToArray();

        // Convert string[] to List<string>
        List<string> result = new List<string>(values);

        return result;
    }
}
