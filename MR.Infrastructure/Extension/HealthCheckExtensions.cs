namespace MR.Infrastructure.Extension;

/// <summary>
/// The two endpoints P2's shared-kernel table requires, and the reason they are two.
/// <para>
/// Before this existed, <c>AddHealthCheck</c>/<c>UseHealthCheck</c> were fully written and
/// never called from <c>Program.cs</c> — so the application exposed no health endpoint at
/// all, and a deploy that came up with an unreachable database looked healthy. The old
/// implementation also read a connection string named <c>OnionArchConn</c>, a key inherited
/// from the tutorial it was adapted from that exists nowhere in this repository, so it would
/// have thrown had anything invoked it.
/// </para>
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>Checks that answer "is this instance ready to take traffic?".</summary>
    private const string ReadyTag = "ready";

    /// <summary>Checks that answer "is this process alive?" — deliberately almost nothing.</summary>
    private const string LiveTag = "live";

    public static IServiceCollection AddDefaultHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Liveness must not depend on anything external. If it did, a database blip
            // would make the orchestrator kill and restart otherwise-healthy processes,
            // turning a dependency outage into an outage of its own.
            .AddCheck(LiveTag, () => HealthCheckResult.Healthy(), tags: new[] { LiveTag })
            .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { ReadyTag });

        return services;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Readiness: every check. This is what a platform health probe points at.
        app.MapHealthChecks("/health");

        // Liveness: the live-tagged checks only.
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(LiveTag),
        });

        return app;
    }
}
