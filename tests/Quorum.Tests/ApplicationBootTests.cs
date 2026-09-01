using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quorum.Infrastructure.Persistence;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// The first tests in this repository that start the application.
/// <para>
/// Everything else here is pure logic — scopes, upload rules, quarter arithmetic — and the
/// architecture review's standing residual risk is that <i>the application has still never
/// been run</i>. The evidence that this matters is F6's <c>FindAsync</c> call: it bound to
/// the <c>params object[]</c> overload, compiled cleanly for three years, and threw the
/// first time it executed. A green build is not evidence that the thing starts.
/// </para>
/// <para>
/// These need no Docker, no database and no network. With no connection string configured
/// the provider switch falls back to InMemory, which is a README claim these tests also
/// happen to verify.
/// </para>
/// </summary>
public sealed class ApplicationBootTests
{
    [Fact]
    public async Task The_application_starts_and_liveness_answers()
    {
        await using var factory = new QuorumApplicationFactory();
        using var client = factory.CreateClient(NoRedirects);

        using var response = await client.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Readiness_becomes_healthy_once_the_schema_has_been_applied()
    {
        await using var factory = new QuorumApplicationFactory();
        using var client = factory.CreateClient(NoRedirects);

        // Readiness is a transition, not a value: the schema is applied by a hosted service
        // *after* the listener is up, so sampling once would race the migration and fail or
        // pass depending on timing. Poll, and report the last status actually seen rather
        // than a bare timeout.
        var deadline = DateTime.UtcNow.AddSeconds(30);
        HttpStatusCode? last = null;

        while (DateTime.UtcNow < deadline)
        {
            using var response = await client.GetAsync("/health");
            last = response.StatusCode;

            if (last == HttpStatusCode.OK)
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Fail($"/health never became healthy within 30s; last status was {last?.ToString() ?? "no response"}.");
    }

    [Fact]
    public async Task Readiness_is_gated_on_the_migration_signal_and_liveness_is_not()
    {
        // The distinction the two endpoints exist for. If liveness depended on the database
        // or the schema, a dependency blip would have the orchestrator kill and restart
        // healthy processes — turning someone else's outage into one of your own.
        //
        // Substituting the signal is what makes this deterministic rather than a race: the
        // hosted service still completes the concrete MigrationCompletionSignal, while the
        // readiness check resolves the interface and never sees it.
        await using var factory = new QuorumApplicationFactory(services =>
        {
            services.RemoveAll<IMigrationCompletionSignal>();
            services.AddSingleton<IMigrationCompletionSignal, SchemaNeverReady>();
        });
        using var client = factory.CreateClient(NoRedirects);

        using var health = await client.GetAsync("/health");
        using var alive = await client.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, health.StatusCode);
        Assert.Equal(HttpStatusCode.OK, alive.StatusCode);
    }

    private static WebApplicationFactoryClientOptions NoRedirects => new() { AllowAutoRedirect = false };

    private sealed class SchemaNeverReady : IMigrationCompletionSignal
    {
        public bool IsCompleted => false;

        public Task WaitAsync(CancellationToken cancellationToken)
            => new TaskCompletionSource().Task.WaitAsync(cancellationToken);
    }
}

/// <summary>
/// Boots <c>Quorum.Server</c> in memory.
/// </summary>
internal sealed class QuorumApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IServiceCollection>? _configureServices;

    public QuorumApplicationFactory(Action<IServiceCollection>? configureServices = null)
        => _configureServices = configureServices;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Startup requires all four: AddExternalJwtAuthentication and AddBffAuthentication
        // each throw naming the setting rather than deferring the failure to the first
        // request, which is the behaviour these values exist to satisfy.
        //
        // Nothing here is contacted. The JWT handler fetches the discovery document lazily,
        // at the first *authenticated* request, and these tests make none — so an
        // unroutable host is the honest choice: if a test ever does authenticate, it will
        // fail loudly rather than quietly reaching something real.
        builder.UseSetting("Auth:MetadataAddress", "https://identity.invalid/.well-known/openid-configuration");
        builder.UseSetting("Auth:Issuer", "https://identity.invalid");
        builder.UseSetting("Auth:Audience", "quorum-tests");
        builder.UseSetting("Auth:ServiceBaseUrl", "https://identity.invalid");

        // Not Development: that branch calls UseWebAssemblyDebugging, which is a debugging
        // proxy this has no use for. The other branch is exception handling and HSTS, both
        // harmless here.
        builder.UseEnvironment("Testing");

        if (_configureServices is not null)
        {
            builder.ConfigureTestServices(_configureServices);
        }
    }
}
