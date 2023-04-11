namespace MR.Infrastructure.Extension;

public static class ConfigureServiceContainer
{
    public static void AddAutoMapper(this IServiceCollection serviceCollection)
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new PaymentProfile());
        });
        IMapper mapper = mappingConfig.CreateMapper();
        serviceCollection.AddSingleton(mapper);
    }

    public static void AddScopedServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());
    }

    public static void AddTransientServices(this IServiceCollection serviceCollection)
    {
    }

    public static void AddSwaggerOpenAPI(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen(setupAction =>
        {

            setupAction.SwaggerDoc(
                "OpenAPISpecification",
                new OpenApiInfo()
                {
                    Title = "Onion Architecture WebAPI",
                    Version = "1",
                    Description = "Through this API you can access customer details",
                    Contact = new OpenApiContact()
                    {
                        Email = "amit.naik8103@gmail.com",
                        Name = "Amit Naik",
                        Url = new Uri("https://amitpnk.github.io/")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

        });

    }

    public static IServiceCollection AddController(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddControllersWithViews().AddNewtonsoftJson();
        return serviceCollection;
    }

    public static void AddVersion(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });
    }

    public static void AddHealthCheck(this IServiceCollection serviceCollection, AppSettings appSettings, IConfiguration configuration)
    {
        serviceCollection.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "Application DB Context", failureStatus: HealthStatus.Degraded)
            .AddUrlGroup(new Uri(appSettings.ApplicationDetail.ContactWebsite), name: "My personal website", failureStatus: HealthStatus.Degraded)
            .AddSqlServer(configuration.GetConnectionString("OnionArchConn"));

        serviceCollection.AddHealthChecksUI(setupSettings: setup =>
        {
            setup.AddHealthCheckEndpoint("Basic Health Check", $"/healthz");
        }).AddInMemoryStorage();
    }
}
