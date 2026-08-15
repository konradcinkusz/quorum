namespace Quorum.Client.Features;

public class RoleAuthorizationMessageHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public RoleAuthorizationMessageHandler(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity.IsAuthenticated && !user.HasClaim(c => c.Type == "role" && StringExt.ExtractStringList(c.Value).Any(d => d == "Admin")))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
