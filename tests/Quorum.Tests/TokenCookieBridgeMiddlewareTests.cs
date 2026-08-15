using Microsoft.AspNetCore.Http;
using Quorum.Infrastructure.Auth;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// The cookie→bearer bridge is the piece that lets the browser hold no token (ADR 0001):
/// these pin the three behaviours that matter — the cookie becomes the Authorization
/// header, an explicit header always wins, and no cookie means no header at all.
/// </summary>
public class TokenCookieBridgeMiddlewareTests
{
    private static DefaultHttpContext ContextWithCookie(string? token)
    {
        var context = new DefaultHttpContext();
        if (token is not null)
        {
            context.Request.Headers.Cookie = $"{AuthCookies.AccessToken}={token}";
        }

        return context;
    }

    [Fact]
    public async Task The_access_cookie_becomes_the_bearer_header()
    {
        var context = ContextWithCookie("token-123");
        var middleware = new TokenCookieBridgeMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("Bearer token-123", context.Request.Headers.Authorization);
    }

    [Fact]
    public async Task An_explicit_authorization_header_is_never_overwritten()
    {
        // Swagger, tests and service callers send their own bearer token; the cookie must
        // not silently replace the credential the caller chose.
        var context = ContextWithCookie("cookie-token");
        context.Request.Headers.Authorization = "Bearer explicit-token";
        var middleware = new TokenCookieBridgeMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("Bearer explicit-token", context.Request.Headers.Authorization);
    }

    [Fact]
    public async Task No_cookie_means_no_header()
    {
        var context = ContextWithCookie(null);
        var middleware = new TokenCookieBridgeMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.False(context.Request.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task The_next_middleware_always_runs()
    {
        var invoked = false;
        var middleware = new TokenCookieBridgeMiddleware(_ => { invoked = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(ContextWithCookie("any"));

        Assert.True(invoked);
    }
}
