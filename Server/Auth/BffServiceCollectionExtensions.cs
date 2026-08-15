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
            });

        services.AddScoped<IBffSessionService, BffSessionService>();

        return services;
    }
}
