using Quorum.Shared.DTOs.Auth;

namespace Quorum.Client.Features.Auth;

/// <summary>Outcome of a BFF auth call the UI can render without exception handling.</summary>
public sealed record BffCallResult<T>(bool Succeeded, T? Value, string? ErrorMessage, int StatusCode)
{
    public static BffCallResult<T> Ok(T value, int statusCode = 200) => new(true, value, null, statusCode);
    public static BffCallResult<T> Fail(string message, int statusCode) => new(false, default, message, statusCode);
}

/// <summary>
/// The client's entire knowledge of authentication: the BFF endpoints on its own origin
/// (ADR 0001). No tokens, no authservice URL, no third-party endpoints — the browser
/// carries an HttpOnly cookie it cannot read, and this client ferries JSON.
/// </summary>
public interface IBffAuthClient
{
    Task<BffSession> GetSessionAsync();
    Task<BffCallResult<BffLoginResult>> LoginAsync(BffLoginRequest request);
    Task<BffCallResult<BffLoginResult>> TwoFactorLoginAsync(BffTwoFactorLoginRequest request);
    Task<BffCallResult<BffRegisterResult>> RegisterAsync(BffRegisterRequest request);
    Task<bool> TryRefreshAsync();
    Task LogoutAsync();
    Task<BffConsentVersions?> GetConsentVersionsAsync();
    Task<BffCallResult<string>> ForgotPasswordAsync(BffForgotPasswordRequest request);
    Task<BffCallResult<string>> ResetPasswordAsync(BffResetPasswordRequest request);
    Task<BffCallResult<string>> VerifyEmailAsync(BffVerifyEmailRequest request);
    Task<BffCallResult<string>> ResendVerificationAsync(BffResendVerificationRequest request);
}

internal sealed class BffAuthClient : IBffAuthClient
{
    private readonly HttpClient _http;

    public BffAuthClient(HttpClient http) => _http = http;

    public async Task<BffSession> GetSessionAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<BffSession>("bff/auth/session") ?? BffSession.Anonymous;
        }
        catch (HttpRequestException)
        {
            // A dead server means an anonymous UI, not an unhandled exception at boot.
            return BffSession.Anonymous;
        }
    }

    public Task<BffCallResult<BffLoginResult>> LoginAsync(BffLoginRequest request)
        => PostAsync<BffLoginRequest, BffLoginResult>("bff/auth/login", request);

    public Task<BffCallResult<BffLoginResult>> TwoFactorLoginAsync(BffTwoFactorLoginRequest request)
        => PostAsync<BffTwoFactorLoginRequest, BffLoginResult>("bff/auth/2fa/login", request);

    public Task<BffCallResult<BffRegisterResult>> RegisterAsync(BffRegisterRequest request)
        => PostAsync<BffRegisterRequest, BffRegisterResult>("bff/auth/register", request);

    public async Task<bool> TryRefreshAsync()
    {
        try
        {
            using var response = await _http.PostAsync("bff/auth/refresh", content: null);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            using var _ = await _http.PostAsync("bff/auth/logout", content: null);
        }
        catch (HttpRequestException)
        {
            // Logout must never strand the user on an error page; server-side cookies are
            // short-lived and the upstream session is revoked at next contact.
        }
    }

    public async Task<BffConsentVersions?> GetConsentVersionsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<BffConsentVersions>("bff/auth/consent-versions");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public Task<BffCallResult<string>> ForgotPasswordAsync(BffForgotPasswordRequest request)
        => PostForMessageAsync("bff/auth/forgot-password", request);

    public Task<BffCallResult<string>> ResetPasswordAsync(BffResetPasswordRequest request)
        => PostForMessageAsync("bff/auth/reset-password", request);

    public Task<BffCallResult<string>> VerifyEmailAsync(BffVerifyEmailRequest request)
        => PostForMessageAsync("bff/auth/verify-email", request);

    public Task<BffCallResult<string>> ResendVerificationAsync(BffResendVerificationRequest request)
        => PostForMessageAsync("bff/auth/resend-verification", request);

    private async Task<BffCallResult<TResult>> PostAsync<TRequest, TResult>(string route, TRequest request)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(route, request);
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<TResult>();
                return value is null
                    ? BffCallResult<TResult>.Fail("The server returned an empty response.", (int)response.StatusCode)
                    : BffCallResult<TResult>.Ok(value, (int)response.StatusCode);
            }

            return BffCallResult<TResult>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return BffCallResult<TResult>.Fail("The server could not be reached. Please try again.", 0);
        }
    }

    private async Task<BffCallResult<string>> PostForMessageAsync<TRequest>(string route, TRequest request)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(route, request);
            var message = await ReadErrorAsync(response);
            return response.IsSuccessStatusCode
                ? BffCallResult<string>.Ok(message, (int)response.StatusCode)
                : BffCallResult<string>.Fail(message, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return BffCallResult<string>.Fail("The server could not be reached. Please try again.", 0);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<BffAuthError>();
            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error!.Message;
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // Not JSON (a proxy error page, an empty body) — fall through to the default.
        }

        return response.IsSuccessStatusCode
            ? "Done."
            : "The request could not be completed.";
    }
}
