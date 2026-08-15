using System.Text.Json;

namespace Quorum.Server.Auth;

/// <summary>
/// The server-to-server client for Quorum's authservice instance — the only place in the
/// codebase that knows authservice's routes. The browser never calls it and never learns
/// its URL (ADR 0001); everything goes through these proxied calls.
/// </summary>
public interface IAuthServiceGateway
{
    Task<AuthGatewayOutcome> LoginAsync(string email, string password, CancellationToken ct);
    Task<AuthGatewayOutcome> TwoFactorLoginAsync(string challengeToken, string? code, string? recoveryCode, CancellationToken ct);
    Task<AuthGatewayOutcome> RefreshAsync(string refreshToken, CancellationToken ct);
    Task<AuthGatewayOutcome> RegisterAsync(BffRegisterRequest request, CancellationToken ct);
    Task LogoutAsync(string accessToken, CancellationToken ct);
    Task<BffConsentVersions?> GetConsentVersionsAsync(CancellationToken ct);
    Task<AuthGatewayMessage> ForgotPasswordAsync(string email, CancellationToken ct);
    Task<AuthGatewayMessage> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct);
    Task<AuthGatewayMessage> VerifyEmailAsync(string email, string token, CancellationToken ct);
    Task<AuthGatewayMessage> ResendVerificationAsync(string email, CancellationToken ct);
}

/// <summary>A token pair as issued by authservice; lives server-side only.</summary>
public sealed record AuthTokens(string AccessToken, string RefreshToken, int ExpiresIn);

/// <summary>
/// One outcome type for every call that can end in a session: tokens, a 2FA challenge, a
/// pending email verification, or a failure with the upstream status and message.
/// </summary>
public sealed record AuthGatewayOutcome
{
    public AuthTokens? Tokens { get; init; }
    public string? ChallengeToken { get; init; }
    public int ChallengeExpiresIn { get; init; }
    public string? PendingVerificationMessage { get; init; }
    public int ErrorStatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public bool Succeeded => Tokens is not null;
    public bool RequiresTwoFactor => ChallengeToken is not null;
    public bool IsPendingVerification => PendingVerificationMessage is not null;
}

public sealed record AuthGatewayMessage(int StatusCode, string Message);

internal sealed class AuthServiceGateway : IAuthServiceGateway
{
    // authservice serialises camelCase; these mirror its documented response DTOs.
    private sealed record TokenResponsePayload(string AccessToken, string RefreshToken, int ExpiresIn, string? TokenType);
    private sealed record TwoFactorPayload(bool RequiresTwoFactor, string ChallengeToken, int ExpiresIn);
    private sealed record PendingPayload(string? UserId, string? Email, string? Message);
    private sealed record MessagePayload(string? Message, string? Error);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ILogger<AuthServiceGateway> _logger;

    public AuthServiceGateway(HttpClient http, ILogger<AuthServiceGateway> logger)
    {
        _http = http;
        _logger = logger;
    }

    public Task<AuthGatewayOutcome> LoginAsync(string email, string password, CancellationToken ct)
        => PostForTokensAsync("api/v1/auth/login", new { email, password }, ct);

    public Task<AuthGatewayOutcome> TwoFactorLoginAsync(
        string challengeToken, string? code, string? recoveryCode, CancellationToken ct)
        => PostForTokensAsync("api/v1/auth/2fa/login", new { challengeToken, code, recoveryCode }, ct);

    public Task<AuthGatewayOutcome> RefreshAsync(string refreshToken, CancellationToken ct)
        => PostForTokensAsync("api/v1/auth/refresh", new { refreshToken }, ct);

    public Task<AuthGatewayOutcome> RegisterAsync(BffRegisterRequest request, CancellationToken ct)
        => PostForTokensAsync("api/v1/auth/register", new
        {
            email = request.Email,
            password = request.Password,
            acceptedTermsVersion = request.AcceptedTermsVersion,
            acceptedPrivacyVersion = request.AcceptedPrivacyVersion,
        }, ct);

    public async Task LogoutAsync(string accessToken, CancellationToken ct)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/logout");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            // Best-effort: the cookies are deleted regardless. Upstream revocation failing
            // must not leave the user unable to log out of this browser.
            using var response = await _http.SendAsync(message, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("authservice logout returned {Status}", (int)response.StatusCode);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "authservice logout failed; local cookies are cleared anyway");
        }
    }

    public async Task<BffConsentVersions?> GetConsentVersionsAsync(CancellationToken ct)
    {
        using var response = await _http.GetAsync("api/v1/auth/consents/versions", ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<BffConsentVersions>(Json, ct);
    }

    public Task<AuthGatewayMessage> ForgotPasswordAsync(string email, CancellationToken ct)
        => PostForMessageAsync("api/v1/auth/forgot-password", new { email }, ct);

    public Task<AuthGatewayMessage> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct)
        => PostForMessageAsync("api/v1/auth/reset-password", new { email, token, newPassword }, ct);

    public Task<AuthGatewayMessage> VerifyEmailAsync(string email, string token, CancellationToken ct)
        => PostForMessageAsync("api/v1/auth/verify-email", new { email, token }, ct);

    public Task<AuthGatewayMessage> ResendVerificationAsync(string email, CancellationToken ct)
        => PostForMessageAsync("api/v1/auth/resend-verification", new { email }, ct);

    private async Task<AuthGatewayOutcome> PostForTokensAsync(string route, object body, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(route, body, Json, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Accepted)
        {
            // Two different 200 shapes (tokens vs. 2FA challenge) plus register's 202 —
            // sniff the discriminating property instead of trusting the status alone.
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (root.TryGetProperty("requiresTwoFactor", out var requires) && requires.GetBoolean())
            {
                var challenge = JsonSerializer.Deserialize<TwoFactorPayload>(payload, Json)!;
                return new AuthGatewayOutcome
                {
                    ChallengeToken = challenge.ChallengeToken,
                    ChallengeExpiresIn = challenge.ExpiresIn,
                };
            }

            if (root.TryGetProperty("accessToken", out _))
            {
                var tokens = JsonSerializer.Deserialize<TokenResponsePayload>(payload, Json)!;
                return new AuthGatewayOutcome
                {
                    Tokens = new AuthTokens(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresIn),
                };
            }

            var pending = JsonSerializer.Deserialize<PendingPayload>(payload, Json);
            return new AuthGatewayOutcome
            {
                PendingVerificationMessage = pending?.Message
                    ?? "Registration accepted. Check your email to verify the account before signing in.",
            };
        }

        return new AuthGatewayOutcome
        {
            ErrorStatusCode = (int)response.StatusCode,
            ErrorMessage = ExtractMessage(payload) ?? DefaultMessageFor((int)response.StatusCode),
        };
    }

    private async Task<AuthGatewayMessage> PostForMessageAsync(string route, object body, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(route, body, Json, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);

        return new AuthGatewayMessage(
            (int)response.StatusCode,
            ExtractMessage(payload) ?? DefaultMessageFor((int)response.StatusCode));
    }

    private static string? ExtractMessage(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            var message = JsonSerializer.Deserialize<MessagePayload>(payload, Json);
            return message?.Message ?? message?.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DefaultMessageFor(int statusCode) => statusCode switch
    {
        401 => "Invalid credentials.",
        403 => "The account is not allowed to sign in. If you just registered, verify your email first.",
        429 => "Too many attempts. Please wait a moment and try again.",
        >= 500 => "The identity service is unavailable. Please try again shortly.",
        _ => "The request could not be completed.",
    };
}
