namespace MR.Client.Features;

public static class EnumParser
{
    public static string GetEnumString<T>(T enumValue) where T : Enum
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException("Type parameter T must be an enum type.");

        return enumValue.ToString().Replace("_", " ");
    }
}
