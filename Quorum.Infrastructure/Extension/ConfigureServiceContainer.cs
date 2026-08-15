namespace Quorum.Infrastructure.Extension;

public static class ConfigureServiceContainer
{
    public static void AddInfrastructureAutoMapper(this IServiceCollection serviceCollection)
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
        // IApplicationDbContext is registered by AddDbContextService, next to the
        // AddDbContext call whose lifetime it has to match. It was registered in both
        // places, with different lifetimes, and which one won depended on the call order in
        // Program.cs — so the duplicate is removed rather than kept in sync.
    }

    public static void AddTransientServices(this IServiceCollection serviceCollection)
    {
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
}
