using Quorum.Infrastructure.Persistence;

namespace Quorum.Infrastructure.Extension;

public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the hosted service that applies the schema after startup, plus the
    /// completion signal that readiness (and anything else that needs the schema) waits on.
    /// </summary>
    public static IServiceCollection AddDatabaseSchemaMigration(this IServiceCollection services)
    {
        services.AddSingleton<MigrationCompletionSignal>();
        services.AddSingleton<IMigrationCompletionSignal>(sp => sp.GetRequiredService<MigrationCompletionSignal>());
        services.AddHostedService<MigrationBackgroundService>();

        return services;
    }
}
