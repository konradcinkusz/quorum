using System.Reflection;

namespace MR.Service;

public static class DependencyInjection
{
    /// <summary>
    /// Nazwy muszą być zgodnę z nazwami kluczy w appsettings
    /// </summary>
    public class ConfigurationVariableNames
    {
        public string connectionStringPROD { get; set; } = "PROD";
        public string connectionStringDEV { get; set; } = "DEV";
    }

    public static void AddServiceLayer(this IServiceCollection services)
    {
        // or you can use assembly in Extension method in Infra layer with below command
        services.AddMediatR(Assembly.GetExecutingAssembly());
    }

    public static IServiceCollection AddDbContextService(
        this IServiceCollection services, 
        IConfiguration configuration,
        ConfigurationVariableNames? configurationVariableNames = default)
    {
        if(configurationVariableNames == null)
        {
            configurationVariableNames = new ConfigurationVariableNames();
        }

        string connectionString = configurationVariableNames.connectionStringPROD;
        var env = configuration["ASPNETCORE_ENVIRONMENT"];
        if (env == "Development")
        {
            connectionString = configurationVariableNames.connectionStringDEV;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.EnableSensitiveDataLogging();
            options.UseSqlServer(configuration.GetConnectionString("DEV"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        }, ServiceLifetime.Transient);

        services.AddTransient<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
