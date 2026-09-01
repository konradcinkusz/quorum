namespace Quorum.Server.Auth;

public static class BffServiceCollectionExtensions
{
    /// <summary>
    /// The BFF half of ADR 0001: the server-to-server client for the authservice instance,
    /// the cookie/session service, and the rotation-grace cache. Token validation is
    /// separate (<see cref="Quorum.Infrastructure.Extension.AuthenticationExtensions"/>) —
    /// this is only the proxy that lets the browser hold no token.
    /// </summary>
    public static IServiceCollection AddBffAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AuthenticationExtensions.SectionName);
        var baseUrl = section["ServiceBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            // Fail at startup naming the setting, rather than at the first login with an
            // unconfigured HttpClient.
            throw new InvalidOperationException(
                $"{AuthenticationExtensions.SectionName}:ServiceBaseUrl is not configured. It must be " +
                "the base URL of this system's authservice instance, e.g. " +
                "https://quorum-authservice.fly.dev — supplied via user-secrets in development " +
                $"or {AuthenticationExtensions.SectionName}__ServiceBaseUrl in a deployed environment.");
        }

        services.AddMemoryCache();

        services.AddHttpClient<IAuthServiceGateway, AuthServiceGateway>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                // Long enough to cover the callee's cold start (P7's diagnostic: the caller's
                // timeout must exceed the cold boot when the callee can scale to zero; the
                // authservice instance pins a machine, but a deploy still restarts it).
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // A redirect between services is always a configuration bug, and a silent
                // 301 turns POST into GET (SERVICE-API-PATTERNS §5). Surface it instead.
                AllowAutoRedirect = false,
            })
            // Checklist item 13. This is the call that matters: login, registration and
            // refresh all cross it, on the synchronous request path, to a service this
            // application does not own. Without it, an identity instance that is restarting
            // or briefly unreachable surfaces to the user as a failed login with no second
            // attempt.
            .AddStandardResilienceHandler(options =>
            {
                // The attempt timeout has to cover the callee's cold start, which is why the
                // client's own timeout above is 30 s. The total has to be larger than one
                // attempt or the options validator rejects the configuration at startup —
                // and the total is what the caller actually waits, so it is bounded rather
                // than left at three times the attempt.
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(70);

                // The circuit breaker's sampling window must be at least twice the attempt
                // timeout, again enforced at startup.
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);

                // Two retries, not the default three: every request across this client is a
                // POST that is not idempotent in the strict sense. Registration is the sharp
                // case — a request that reached authservice and created the account, then
                // timed out on the way back, returns a deterministic conflict on retry
                // rather than a second account, so retrying is safe but not free. Two
                // attempts absorb a restart; more just lengthens what the user waits for.
                options.Retry.MaxRetryAttempts = 2;
            });

        services.AddScoped<IBffSessionService, BffSessionService>();

        return services;
    }
}
