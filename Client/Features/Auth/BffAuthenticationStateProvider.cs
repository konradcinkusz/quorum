using System.Security.Claims;
using Quorum.Shared.DTOs.Auth;

namespace Quorum.Client.Features.Auth;

/// <summary>
/// Authentication state from <c>GET /bff/auth/session</c> (ADR 0001). The browser cannot
/// read the HttpOnly cookies back — by design — so the server answers "who am I?" and this
/// provider turns the answer into the <see cref="ClaimsPrincipal"/> the rest of the UI
/// consumes through <c>AuthorizeView</c> and policies.
/// <para>
/// Claim spellings deliberately match what the old token-based provider exposed —
/// <c>name</c>, <c>role</c>, <c>isActiveSubscription</c> — so pages reading them
/// (AuthHelper, NavMenu) did not have to change.
/// </para>
/// </summary>
public sealed class BffAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IBffAuthClient _client;
    private Task<AuthenticationState>? _current;

    public BffAuthenticationStateProvider(IBffAuthClient client) => _client = client;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => _current ??= LoadAsync();

    /// <summary>Publishes a session the caller already holds (login/registration response),
    /// saving the extra round trip.</summary>
    public void NotifySessionChanged(BffSession session)
    {
        _current = Task.FromResult(new AuthenticationState(ToPrincipal(session)));
        NotifyAuthenticationStateChanged(_current);
    }

    /// <summary>Re-asks the server; used after logout and after a failed refresh.</summary>
    public Task ReloadSessionAsync()
    {
        _current = LoadAsync();
        NotifyAuthenticationStateChanged(_current);
        return _current;
    }

    private async Task<AuthenticationState> LoadAsync()
        => new(ToPrincipal(await _client.GetSessionAsync()));

    private static ClaimsPrincipal ToPrincipal(BffSession session)
    {
        if (!session.IsAuthenticated || string.IsNullOrWhiteSpace(session.UserId))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var claims = new List<Claim>
        {
            new("nameid", session.UserId!),
            // The old IdentityServer tokens carried the email in `name` (UserName was the
            // email); pages display it, so the equivalence is preserved.
            new("name", session.Email ?? session.UserName ?? string.Empty),
            new("isActiveSubscription", session.IsActiveSubscription.ToString()),
        };

        if (!string.IsNullOrWhiteSpace(session.Email))
        {
            claims.Add(new Claim("email", session.Email!));
        }

        claims.AddRange(session.Roles.Select(role => new Claim("role", role)));

        var identity = new ClaimsIdentity(claims, authenticationType: "bff", nameType: "name", roleType: "role");
        return new ClaimsPrincipal(identity);
    }
}
