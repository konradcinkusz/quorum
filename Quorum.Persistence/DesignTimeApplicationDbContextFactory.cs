using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Quorum.Persistence;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the context without booting the Server or
/// touching a live database. The provider and migrations assembly come from the same
/// environment variables the runtime uses (<c>DATABASE_PROVIDER</c>,
/// <c>Database__MigrationsAssembly</c>); the connection string is a placeholder because
/// migration generation reads the model, not the database. See
/// <c>scripts/generate-migrations.sh</c>.
/// </summary>
public sealed class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var provider = DatabaseProviderExtensions.ResolveProvider(configuration);
        if (provider == DatabaseProviderExtensions.Provider.InMemory)
        {
            // Design time is always about a relational schema; InMemory has none.
            provider = DatabaseProviderExtensions.Provider.PostgreSql;
        }

        var connectionString = DatabaseProviderExtensions.ResolveConnectionString(configuration)
            ?? (provider == DatabaseProviderExtensions.Provider.SqlServer
                ? "Server=localhost;Database=quorum-design;Trusted_Connection=True;TrustServerCertificate=True;"
                : "Host=localhost;Database=quorum-design;Username=quorum;Password=quorum");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>();
        DatabaseProviderExtensions.ConfigureProvider(options, provider, connectionString, configuration);

        return new ApplicationDbContext(options.Options);
    }
}
