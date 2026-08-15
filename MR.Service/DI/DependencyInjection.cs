namespace MR.Service.DI;

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

    public class ConfigurationSectionNames
    {
        public const string CloudinaryOptions = "CloudinaryOpt";
    }

    public static void AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // GetSection never returns null — it returns an empty section for a missing key — so
        // the old `if (configSection != null)` was always true and the registration was not
        // actually conditional. Now that the credentials live outside appsettings.json, that
        // matters: without this check the Cloudinary client would be constructed with empty
        // strings and fail with an opaque argument error at the first upload.
        var configSection = configuration.GetSection(ConfigurationSectionNames.CloudinaryOptions);
        services.Configure<CloudinaryOpt>(configSection);

        var isCloudinaryConfigured =
            !string.IsNullOrWhiteSpace(configSection[nameof(CloudinaryOpt.Cloud)]) &&
            !string.IsNullOrWhiteSpace(configSection[nameof(CloudinaryOpt.ApiKey)]) &&
            !string.IsNullOrWhiteSpace(configSection[nameof(CloudinaryOpt.ApiSecret)]);

        if (isCloudinaryConfigured)
        {
            services.AddScoped<ICloudinaryService, CloudinaryService>();
        }
        else
        {
            services.AddScoped<ICloudinaryService, CloudinaryNotConfiguredService>();
        }

        services.AddScoped<IIssuePDFService, IssuePDFService>();

        // or you can use assembly in Extension method in Infra layer with below command
        services.AddMediatR(Assembly.GetExecutingAssembly());
    }

    public static IServiceCollection AddDbContextService(
        this IServiceCollection services,
        IConfiguration configuration,
        ConfigurationVariableNames? configurationVariableNames = default)
    {
        if (configurationVariableNames == null)
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
