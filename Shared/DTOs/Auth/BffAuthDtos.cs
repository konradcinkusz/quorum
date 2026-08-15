namespace Quorum.Shared.DTOs.Auth;

/// <summary>
/// The contract between the Blazor client and Quorum.Server's BFF auth endpoints
/// (ADR 0001). The client never talks to authservice and never sees a token; these shapes
/// are what crosses the browser boundary instead.
/// </summary>
public record BffLoginRequest(string Email, string Password);

public record BffTwoFactorLoginRequest(string ChallengeToken, string? Code, string? RecoveryCode);

public record BffRegisterRequest(
    string Email,
    string Password,
    string AcceptedTermsVersion,
    string AcceptedPrivacyVersion);

public record BffForgotPasswordRequest(string Email);

public record BffResetPasswordRequest(string Email, string Token, string NewPassword);

public record BffVerifyEmailRequest(string Email, string Token);

public record BffResendVerificationRequest(string Email);

/// <summary>What the client knows about the signed-in user; rebuilt from
/// <c>GET /bff/auth/session</c> on every page load because client JS cannot read the
/// HttpOnly cookies back — by design.</summary>
public record BffSession
{
    public bool IsAuthenticated { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? UserName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>Quorum's own fact, from Quorum's Subscription table — not an identity claim.
    /// Replaces the <c>isActiveSubscription</c> claim the old in-process IdentityServer
    /// minted into its tokens.</summary>
    public bool IsActiveSubscription { get; init; }

    public static BffSession Anonymous { get; } = new();
}

/// <summary>Result of login (and of the 2FA completion step).</summary>
public record BffLoginResult
{
    /// <summary>True when credentials were right but a TOTP code is still needed;
    /// <see cref="ChallengeToken"/> then rides to the 2FA step.</summary>
    public bool RequiresTwoFactor { get; init; }
    public string? ChallengeToken { get; init; }
    public int ChallengeExpiresIn { get; init; }

    public BffSession? Session { get; init; }
}

/// <summary>Result of registration. Either a live session (cookies already set) or a
/// pending-verification notice when the identity service requires email confirmation.</summary>
public record BffRegisterResult
{
    public bool RequiresEmailVerification { get; init; }
    public string? Message { get; init; }
    public BffSession? Session { get; init; }
}

/// <summary>The consent document versions the identity service currently requires;
/// registration must echo them exactly.</summary>
public record BffConsentVersions(string Terms, string Privacy, string Cookies);

/// <summary>Uniform error body for failed BFF auth calls.</summary>
public record BffAuthError(string Message);
