namespace Quorum.Service.DI;

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

    /// <summary>
    /// The connection-string key to prefer in every environment. The DEV/PROD pair in
    /// <see cref="ConfigurationVariableNames"/> is retained only so existing local setups
    /// keep working.
    /// </summary>
    public const string DefaultConnectionStringName = "Default";

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

    public static IServiceCollection AddDbContextService(
        this IServiceCollection services,
        IConfiguration configuration,
        ConfigurationVariableNames? configurationVariableNames = default)
    {
        configurationVariableNames ??= new ConfigurationVariableNames();

        var environment = configuration["ASPNETCORE_ENVIRONMENT"];
        var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

        // This method used to compute the environment-appropriate key into a local and then
        // pass the literal "DEV" to UseSqlServer, so the PROD key was unreachable: every
        // environment ran against whatever sat in the DEV slot. `Default` is now preferred
        // over both, per P5 — one key supplied per environment beats a key chosen by a
        // branch — with the old DEV/PROD names still honoured so existing setups keep working.
        var environmentSpecificKey = isDevelopment
            ? configurationVariableNames.connectionStringDEV
            : configurationVariableNames.connectionStringPROD;

        var connectionString =
            configuration.GetConnectionString(DefaultConnectionStringName)
            ?? configuration.GetConnectionString(environmentSpecificKey);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Fail at startup with the key names rather than at the first query with a null
            // reference. Previously a missing connection string surfaced as every request
            // returning 500 with nothing pointing at configuration.
            throw new InvalidOperationException(
                $"No database connection string found. Set ConnectionStrings:{DefaultConnectionStringName} " +
                $"(or ConnectionStrings:{environmentSpecificKey} for the '{environment ?? "Production"}' " +
                "environment) via user-secrets, appsettings, or the environment.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Logs parameter values — email addresses, petition text, payment references.
            // Development only.
            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
            }

            options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Scoped, matching AddDbContext's default: a Transient DbContext gives every
        // injection site its own change tracker, so a handler that resolves the context
        // twice cannot see its own pending writes.
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
