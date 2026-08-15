namespace Quorum.Infrastructure.Extension;

public class EnumHelper
{
    public static string EnumToString<T>(T value) where T : Enum
    {
        return Enum.GetName(typeof(T), value);
    }
}
