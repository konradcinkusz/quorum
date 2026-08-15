using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Quorum.Persistence;

/// <summary>
/// P4: the database provider is a configuration switch, not a compile-time decision.
/// <para>
/// <c>DATABASE_PROVIDER</c> (or <c>DatabaseProvider</c>) selects PostgreSQL — the deployed
/// default — or SQL Server, the path local development on Windows has always used. With no
/// connection string configured at all the context falls back to the InMemory provider, so
/// <c>git clone &amp;&amp; dotnet run</c> works with zero infrastructure (P8) and the test
/// suite needs no container.
/// </para>
/// <para>
/// Each relational provider gets its own migrations assembly, because one migration set
/// cannot serve two dialects. <c>Database:MigrationsAssembly</c> overrides the default
/// pairing when generating migrations.
/// </para>
/// </summary>
public static class DatabaseProviderExtensions
{
    public const string PostgreSqlMigrationsAssembly = "Quorum.Persistence.Migrations.PostgreSQL";
    public const string SqlServerMigrationsAssembly = "Quorum.Persistence.Migrations.SqlServer";

    /// <summary>The connection-string key to prefer in every environment.</summary>
    public const string DefaultConnectionStringName = "Default";

    public enum Provider
    {
        InMemory,
        PostgreSql,
        SqlServer,
    }

    public static Provider ResolveProvider(IConfiguration configuration)
    {
        var configured = configuration["DATABASE_PROVIDER"] ?? configuration["DatabaseProvider"];

        if (string.Equals(configured, "SqlServer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(configured, "MsSql", StringComparison.OrdinalIgnoreCase))
        {
            return Provider.SqlServer;
        }

        if (string.Equals(configured, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            return Provider.InMemory;
        }

        // PostgreSQL is the deployed default — but only when there is something to connect
        // to. No connection string means the InMemory fallback, not a connection error at
        // the first query.
        return ResolveConnectionString(configuration) is null ? Provider.InMemory : Provider.PostgreSql;
    }

    public static string? ResolveConnectionString(IConfiguration configuration)
    {
        // `Default` is preferred; the DEV/PROD pair is retained only so existing local
        // setups keep working.
        var environment = configuration["ASPNETCORE_ENVIRONMENT"];
        var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

        var value = configuration.GetConnectionString(DefaultConnectionStringName)
            ?? configuration.GetConnectionString(isDevelopment ? "DEV" : "PROD");

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static string ResolveMigrationsAssembly(IConfiguration configuration, Provider provider)
        => configuration["Database:MigrationsAssembly"] is { Length: > 0 } configured
            ? configured
            : provider == Provider.SqlServer
                ? SqlServerMigrationsAssembly
                : PostgreSqlMigrationsAssembly;

    public static IServiceCollection AddQuorumDbContext(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
    {
        var provider = ResolveProvider(configuration);
        var connectionString = ResolveConnectionString(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Logs parameter values — email addresses, petition text, payment references.
            // Development only.
            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
            }

            ConfigureProvider(options, provider, connectionString, configuration);
        });

        // Scoped, matching AddDbContext's default: a Transient DbContext gives every
        // injection site its own change tracker, so a handler that resolves the context
        // twice cannot see its own pending writes.
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    public static void ConfigureProvider(
        DbContextOptionsBuilder options,
        Provider provider,
        string? connectionString,
        IConfiguration configuration)
    {
        switch (provider)
        {
            case Provider.SqlServer:
                RequireConnectionString(connectionString, "SqlServer");
                options.UseSqlServer(connectionString, sql => sql
                    .MigrationsAssembly(ResolveMigrationsAssembly(configuration, provider))
                    .EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)
                    .CommandTimeout(60));
                break;

            case Provider.PostgreSql:
                RequireConnectionString(connectionString, "PostgreSQL");
                options.UseNpgsql(connectionString, sql => sql
                    .MigrationsAssembly(ResolveMigrationsAssembly(configuration, provider))
                    .EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null)
                    .CommandTimeout(60));
                break;

            default:
                options.UseInMemoryDatabase("QuorumInMemory");
                break;
        }
    }

    private static void RequireConnectionString(string? connectionString, string providerName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Fail at startup with the key names rather than at the first query. A provider
            // was asked for by name, so silently degrading to InMemory would hide a broken
            // deployment behind an empty database.
            throw new InvalidOperationException(
                $"DATABASE_PROVIDER is {providerName} but no connection string is configured. " +
                $"Set ConnectionStrings:{DefaultConnectionStringName} via user-secrets, " +
                "appsettings, or the environment.");
        }
    }
}
