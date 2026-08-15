namespace Quorum.Infrastructure.Auth;

/// <summary>
/// Translates the BFF's HttpOnly access-token cookie into the <c>Authorization</c> header
/// the JWT bearer handler validates.
/// <para>
/// This is the piece that lets the browser hold no token (ADR 0001): the client sends the
/// cookie because it can do nothing else, and the API keeps exactly one authentication
/// path — bearer JWT, validated against authservice's JWKS. A caller that already sends
/// its own <c>Authorization</c> header (Swagger, a service, a test) is left untouched.
/// </para>
/// <para>
/// CSRF: both cookies are <c>SameSite=Strict</c>, so a cross-site page never gets the
/// browser to attach them; nothing here weakens that. The cookie value is not trusted by
/// this middleware in any way — it is handed to the same validation an explicit bearer
/// token would get.
/// </para>
/// </summary>
public sealed class TokenCookieBridgeMiddleware
{
    private readonly RequestDelegate _next;

    public TokenCookieBridgeMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization")
            && context.Request.Cookies.TryGetValue(AuthCookies.AccessToken, out var token)
            && !string.IsNullOrWhiteSpace(token))
        {
            context.Request.Headers.Authorization = $"Bearer {token}";
        }

        return _next(context);
    }
}

public static class TokenCookieBridgeMiddlewareExtensions
{
    /// <summary>Must run before <c>UseAuthentication()</c>, or the bearer handler never sees the cookie.</summary>
    public static IApplicationBuilder UseTokenCookieBridge(this IApplicationBuilder app)
        => app.UseMiddleware<TokenCookieBridgeMiddleware>();
}
