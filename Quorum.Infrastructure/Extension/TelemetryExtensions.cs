using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Quorum.Infrastructure.Extension;

/// <summary>
/// P15 — observability is a build-time decision. The application previously emitted no
/// traces or metrics at all and logged only to the default console provider, so a payment
/// that was accepted while its issue failed to publish left nothing to follow: no trace, no
/// correlation between the client call and the handler, and diagnosis meant reproducing it
/// locally.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>Set by the platform. Its absence is how a developer runs with no collector.</summary>
    private const string OtlpEndpointVariable = "OTEL_EXPORTER_OTLP_ENDPOINT";

    private static readonly string[] ProbePaths = new[] { "/health", "/alive" };

    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: builder.Environment.ApplicationName,
                serviceVersion: typeof(TelemetryExtensions).Assembly.GetName().Version?.ToString()))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                    // Probes run every few seconds forever. Left in, they dominate the trace
                    // volume and bury the requests anyone actually wants to read.
                    options.Filter = context => !IsProbe(context.Request.Path))
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation());

        // The whole integration is conditional on its configuration being present (P8):
        // with no endpoint set, instrumentation still runs but nothing is exported, so
        // `git clone && dotnet run` works with no collector and no cloud credentials.
        if (!string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointVariable]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    private static bool IsProbe(PathString path)
        => ProbePaths.Any(probe => path.StartsWithSegments(probe, StringComparison.OrdinalIgnoreCase));
}
