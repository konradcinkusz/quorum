namespace Quorum.Client.Features;

public static class AuthHelper
{
    public static Tuple<bool, string> IsActiveSubscription(AuthenticationState? authState)
    {
        if (authState == null)
        {
            return new Tuple<bool, string>(false, string.Empty);
        }
        var user = authState.User;
        var isActiveSubscriptionClaim = user.Claims.FirstOrDefault(x => x.Type == "isActiveSubscription");
        var isActiveSubscription = bool.TryParse(isActiveSubscriptionClaim?.Value, out bool claimValue) ? claimValue : false;
        var email = user.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? string.Empty;
        return new Tuple<bool, string>(isActiveSubscription, email);
    }

    public static bool IsLogged(AuthenticationState? authState, NavigationManager navigationManager)
    {
        if (!authState.User.Identity.IsAuthenticated)
        {
            navigationManager.NavigateTo("/login");
            return false;
        }
        return true;
    }
}
