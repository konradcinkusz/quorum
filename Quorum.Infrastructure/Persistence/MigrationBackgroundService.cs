using Microsoft.Extensions.Hosting;

namespace Quorum.Infrastructure.Persistence;

/// <summary>
/// Applies the schema after Kestrel is already listening, so a slow first boot — cold
/// database, long migration — is not read by the platform as a failed deploy (P4,
/// FLY-IO-DEPLOYMENT §2). Readiness is what reports the truth in the meantime: the
/// migrations health check stays unhealthy until <see cref="MigrationCompletionSignal"/>
/// completes.
/// <para>
/// Relational providers are migrated — never <c>EnsureCreated</c>, which records no
/// migration history and freezes the schema at first-boot state. The InMemory fallback has
/// no migrations to apply, so it is the one path that uses <c>EnsureCreated</c>.
/// </para>
/// </summary>
public sealed class MigrationBackgroundService : BackgroundService
{
    private const int MaxAttempts = 10;

    private readonly IServiceProvider _serviceProvider;
    private readonly MigrationCompletionSignal _signal;
    private readonly ILogger<MigrationBackgroundService> _logger;

    public MigrationBackgroundService(
        IServiceProvider serviceProvider,
        MigrationCompletionSignal signal,
        ILogger<MigrationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _signal = signal;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                // A fresh scope per attempt: a DbContext that has faulted once (broken
                // connection mid-migration) is not something to retry on.
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (context.Database.IsRelational())
                {
                    _logger.LogInformation(
                        "Applying database migrations (provider {Provider}, attempt {Attempt}/{Max})",
                        context.Database.ProviderName, attempt, MaxAttempts);
                    await context.Database.MigrateAsync(stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Non-relational provider; ensuring the in-memory store exists");
                    await context.Database.EnsureCreatedAsync(stoppingToken);
                }

                _signal.MarkCompleted();
                _logger.LogInformation("Database schema is ready");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // The listener is already up, so a transient failure here must not take the
                // process down; readiness keeps reporting unhealthy until this succeeds.
                var delay = TimeSpan.FromSeconds(Math.Min(5 * attempt, 30));
                _logger.LogError(ex,
                    "Schema initialization attempt {Attempt}/{Max} failed; retrying in {Delay}s",
                    attempt, MaxAttempts, delay.TotalSeconds);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        _logger.LogError(
            "Schema initialization failed after {Max} attempts. The service keeps running but " +
            "readiness will report unhealthy until the database is reachable and the service restarts.",
            MaxAttempts);
    }
}
