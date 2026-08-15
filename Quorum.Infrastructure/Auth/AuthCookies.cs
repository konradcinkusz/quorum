namespace Quorum.Infrastructure.Auth;

/// <summary>
/// The two HttpOnly cookies the BFF issues (ADR 0001, FRONTEND-BFF §3). Client JavaScript
/// never reads either — the browser carries them, the server translates.
/// </summary>
public static class AuthCookies
{
    /// <summary>The authservice-issued access token; sent on every same-origin request.</summary>
    public const string AccessToken = "quorum.access";

    /// <summary>
    /// The rotating refresh token. Scoped to the BFF's own path so it travels only to the
    /// endpoints that actually redeem it, not to every API call.
    /// </summary>
    public const string RefreshToken = "quorum.refresh";

    /// <summary>The path the refresh cookie is scoped to. Deleting a cookie requires the
    /// same attributes it was set with, so the value lives here, once.</summary>
    public const string RefreshTokenPath = "/bff/auth";
}
