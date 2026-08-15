namespace Quorum.Domain.Constants;

public static partial class Constants
{
    public static class RouteValues
    {
        public const string BasicAPIRoute = "api/[controller]";
        public const string AdvanceAPIRoute = "api/v{version:apiVersion}/[controller]";
        public const string APIV1 = "1.0";
    }
}