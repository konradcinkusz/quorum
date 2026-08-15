using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Quorum.Server.Controllers;

/// <summary>
/// The BFF face of authservice (ADR 0001): the browser talks only to its own origin, these
/// endpoints proxy to Quorum's authservice instance, and the tokens ride in HttpOnly
/// cookies that client JavaScript never sees. Deliberately outside the versioned
/// <c>/api/v1.0</c> surface — this is the session plumbing of this frontend, not part of
/// Quorum's domain API.
/// </summary>
[ApiController]
[Route("bff/auth")]
public class BffAuthController : ControllerBase
{
    private readonly IAuthServiceGateway _gateway;
    private readonly IBffSessionService _session;

    public BffAuthController(IAuthServiceGateway gateway, IBffSessionService session)
    {
        _gateway = gateway;
        _session = session;
    }

    /// <summary>Rehydrates client-side auth state. Anonymous callers get an anonymous
    /// session, not a 401 — page load must be able to ask "who am I?" cheaply.</summary>
    [HttpGet("session")]
    [AllowAnonymous]
    public async Task<ActionResult<BffSession>> Session(CancellationToken ct)
        => await _session.BuildSessionAsync(HttpContext, ct);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<BffLoginResult>> Login(BffLoginRequest request, CancellationToken ct)
    {
        var outcome = await _gateway.LoginAsync(request.Email, request.Password, ct);
        return await ToLoginResultAsync(outcome, ct);
    }

    /// <summary>Completes a login that answered with a 2FA challenge.</summary>
    [HttpPost("2fa/login")]
    [AllowAnonymous]
    public async Task<ActionResult<BffLoginResult>> TwoFactorLogin(BffTwoFactorLoginRequest request, CancellationToken ct)
    {
        var outcome = await _gateway.TwoFactorLoginAsync(request.ChallengeToken, request.Code, request.RecoveryCode, ct);
        return await ToLoginResultAsync(outcome, ct);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<BffRegisterResult>> Register(BffRegisterRequest request, CancellationToken ct)
    {
        var outcome = await _gateway.RegisterAsync(request, ct);

        if (outcome.Succeeded)
        {
            var session = await _session.EstablishSessionAsync(HttpContext, outcome.Tokens!, ct);
            return new BffRegisterResult { Session = session };
        }

        if (outcome.IsPendingVerification)
        {
            return Accepted(new BffRegisterResult
            {
                RequiresEmailVerification = true,
                Message = outcome.PendingVerificationMessage,
            });
        }

        return Error(outcome);
    }

    /// <summary>Redeems the refresh cookie for a fresh pair; 401 ends the client session.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<BffSession>> Refresh(CancellationToken ct)
    {
        var session = await _session.RefreshAsync(HttpContext, ct);
        return session is null
            ? Unauthorized(new BffAuthError("The session has expired. Please sign in again."))
            : session;
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _session.EndSessionAsync(HttpContext, ct);
        return Ok();
    }

    /// <summary>The consent versions registration must echo; fetched live so a version bump
    /// upstream cannot strand the signup form.</summary>
    [HttpGet("consent-versions")]
    [AllowAnonymous]
    public async Task<ActionResult<BffConsentVersions>> ConsentVersions(CancellationToken ct)
    {
        var versions = await _gateway.GetConsentVersionsAsync(ct);
        return versions is null
            ? StatusCode(StatusCodes.Status502BadGateway,
                new BffAuthError("The identity service is unavailable. Please try again shortly."))
            : versions;
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(BffForgotPasswordRequest request, CancellationToken ct)
        => Relay(await _gateway.ForgotPasswordAsync(request.Email, ct));

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(BffResetPasswordRequest request, CancellationToken ct)
        => Relay(await _gateway.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, ct));

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(BffVerifyEmailRequest request, CancellationToken ct)
        => Relay(await _gateway.VerifyEmailAsync(request.Email, request.Token, ct));

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification(BffResendVerificationRequest request, CancellationToken ct)
        => Relay(await _gateway.ResendVerificationAsync(request.Email, ct));

    private async Task<ActionResult<BffLoginResult>> ToLoginResultAsync(AuthGatewayOutcome outcome, CancellationToken ct)
    {
        if (outcome.RequiresTwoFactor)
        {
            // The challenge token is not an access credential — it can only be redeemed at
            // the 2FA step together with a valid code — so it may cross to the client.
            return new BffLoginResult
            {
                RequiresTwoFactor = true,
                ChallengeToken = outcome.ChallengeToken,
                ChallengeExpiresIn = outcome.ChallengeExpiresIn,
            };
        }

        if (outcome.Succeeded)
        {
            var session = await _session.EstablishSessionAsync(HttpContext, outcome.Tokens!, ct);
            return new BffLoginResult { Session = session };
        }

        return Error(outcome);
    }

    private ObjectResult Error(AuthGatewayOutcome outcome)
        => StatusCode(
            outcome.ErrorStatusCode is >= 400 and < 600 ? outcome.ErrorStatusCode : StatusCodes.Status502BadGateway,
            new BffAuthError(outcome.ErrorMessage ?? "The request could not be completed."));

    private IActionResult Relay(AuthGatewayMessage message)
        => StatusCode(
            message.StatusCode is >= 200 and < 600 ? message.StatusCode : StatusCodes.Status502BadGateway,
            new BffAuthError(message.Message));
}
