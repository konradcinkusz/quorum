namespace Quorum.Service.DI;

public static class DependencyInjection
{
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

        // MR's own view of a user — subscription state, the roster, per-user provisioning.
        // Deliberately independent of whether identity is local or lives in authservice.
        services.AddScoped<IQuorumUserService, QuorumUserService>();

        // or you can use assembly in Extension method in Infra layer with below command
        services.AddMediatR(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Registers the DbContext behind the P4 provider switch: PostgreSQL (deployed default),
    /// SQL Server (local development), or InMemory when nothing is configured. The provider
    /// logic itself lives next to the context, in
    /// <see cref="DatabaseProviderExtensions.AddQuorumDbContext"/>.
    /// </summary>
    public static IServiceCollection AddDbContextService(
        this IServiceCollection services, IConfiguration configuration)
    {
        var isDevelopment = string.Equals(
            configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase);

        return services.AddQuorumDbContext(configuration, isDevelopment);
    }
}
