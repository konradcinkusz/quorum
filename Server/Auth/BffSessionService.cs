using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Memory;

namespace Quorum.Server.Auth;

/// <summary>
/// Everything between an authservice token pair and the browser: cookie issuance, session
/// payloads, provisioning, and the serialized refresh. The controller stays transport-only.
/// </summary>
public interface IBffSessionService
{
    /// <summary>Sets the cookie pair and returns the session for the freshly issued tokens,
    /// provisioning Quorum-side state for a first-seen user.</summary>
    Task<BffSession> EstablishSessionAsync(HttpContext httpContext, AuthTokens tokens, CancellationToken ct);

    /// <summary>Builds the session for an already-authenticated request (the cookie survived
    /// validation), provisioning Quorum-side state for a first-seen user.</summary>
    Task<BffSession> BuildSessionAsync(HttpContext httpContext, CancellationToken ct);

    /// <summary>Redeems the refresh cookie for a new pair. Serialized per token: authservice
    /// rotates on every use and treats a replay as theft, so two racing browser tabs must
    /// not both reach it with the same token.</summary>
    Task<BffSession?> RefreshAsync(HttpContext httpContext, CancellationToken ct);

    /// <summary>Revokes upstream (best-effort) and deletes both cookies.</summary>
    Task EndSessionAsync(HttpContext httpContext, CancellationToken ct);
}

internal sealed class BffSessionService : IBffSessionService
{
    /// <summary>How long a redeemed refresh token maps to its replacement pair, so a second
    /// request that raced the rotation gets the same new cookies instead of tripping
    /// authservice's replay detection and revoking the whole session family.</summary>
    private static readonly TimeSpan RotationGrace = TimeSpan.FromSeconds(30);

    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private readonly IAuthServiceGateway _gateway;
    private readonly IQuorumUserService _users;
    private readonly IMemoryCache _rotationCache;
    private readonly int _refreshCookieDays;

    public BffSessionService(
        IAuthServiceGateway gateway,
        IQuorumUserService users,
        IMemoryCache rotationCache,
        IConfiguration configuration)
    {
        _gateway = gateway;
        _users = users;
        _rotationCache = rotationCache;
        _refreshCookieDays = configuration.GetSection(AuthenticationExtensions.SectionName)
            .GetValue("RefreshCookieDays", 7);
    }

    public async Task<BffSession> EstablishSessionAsync(
        HttpContext httpContext, AuthTokens tokens, CancellationToken ct)
    {
        WriteCookies(httpContext, tokens);

        // The token came out of authservice over TLS a moment ago; reading its claims here
        // is bookkeeping, not trust — API requests still go through full JWKS validation.
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);

        var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid")?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var userName = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
        var roles = jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).Distinct().ToList();

        return await BuildAsync(userId, email, userName, roles, ct);
    }

    public async Task<BffSession> BuildSessionAsync(HttpContext httpContext, CancellationToken ct)
    {
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return BffSession.Anonymous;
        }

        // Inbound claim mapping is on, so the long URIs are the spellings that exist here.
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("unique_name")?.Value;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Concat(user.FindAll("role").Select(c => c.Value))
            .Distinct().ToList();

        return await BuildAsync(userId, email, userName, roles, ct);
    }

    public async Task<BffSession?> RefreshAsync(HttpContext httpContext, CancellationToken ct)
    {
        var refreshToken = httpContext.Request.Cookies[AuthCookies.RefreshToken];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        // One refresh at a time per instance. Cheap, and combined with the rotation-grace
        // cache it keeps a burst of parallel 401-retries from burning the token family.
        await RefreshLock.WaitAsync(ct);
        try
        {
            if (_rotationCache.TryGetValue<AuthTokens>(CacheKey(refreshToken), out var reissued) && reissued is not null)
            {
                return await EstablishSessionAsync(httpContext, reissued, ct);
            }

            var outcome = await _gateway.RefreshAsync(refreshToken, ct);
            if (!outcome.Succeeded)
            {
                DeleteCookies(httpContext);
                return null;
            }

            _rotationCache.Set(CacheKey(refreshToken), outcome.Tokens, RotationGrace);
            return await EstablishSessionAsync(httpContext, outcome.Tokens!, ct);
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    public async Task EndSessionAsync(HttpContext httpContext, CancellationToken ct)
    {
        var accessToken = httpContext.Request.Cookies[AuthCookies.AccessToken];
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            await _gateway.LogoutAsync(accessToken, ct);
        }

        DeleteCookies(httpContext);
    }

    private async Task<BffSession> BuildAsync(
        string? userId, string? email, string? userName, List<string> roles, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BffSession.Anonymous;
        }

        // The moment Quorum learns a user exists. Registration happens in authservice and
        // Quorum is never told, so first-sight provisioning hangs off the session instead
        // (subscription row, signature pools for open quarters — see QuorumUserService).
        await _users.EnsureProvisionedAsync(userId, email, ct);

        return new BffSession
        {
            IsAuthenticated = true,
            UserId = userId,
            Email = email,
            UserName = userName ?? email,
            Roles = roles,
            IsActiveSubscription = await _users.HasActiveSubscriptionAsync(userId, ct),
        };
    }

    private void WriteCookies(HttpContext httpContext, AuthTokens tokens)
    {
        // HttpOnly + Secure + SameSite=Strict per ADR 0001: client JavaScript can never
        // read these, and a cross-site page can never get the browser to send them.
        httpContext.Response.Cookies.Append(AuthCookies.AccessToken, tokens.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = TimeSpan.FromSeconds(tokens.ExpiresIn),
        });

        httpContext.Response.Cookies.Append(AuthCookies.RefreshToken, tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            // Scoped so the refresh token travels only to the BFF's own endpoints.
            Path = AuthCookies.RefreshTokenPath,
            MaxAge = TimeSpan.FromDays(_refreshCookieDays),
        });
    }

    private static void DeleteCookies(HttpContext httpContext)
    {
        // Deletion must repeat the attributes the cookies were set with — a mismatched
        // path or SameSite silently leaves the cookie alive (ADR 0001).
        httpContext.Response.Cookies.Delete(AuthCookies.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
        });

        httpContext.Response.Cookies.Delete(AuthCookies.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = AuthCookies.RefreshTokenPath,
        });
    }

    private static string CacheKey(string refreshToken)
        => "bff-rotation:" + Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(refreshToken)));
}
