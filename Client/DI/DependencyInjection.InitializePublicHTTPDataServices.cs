namespace MR.Client.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection InitializePublicHTTPDataServices(this IServiceCollection service, Uri baseAddress)
    {
        const string MRPublicDataService = "MR.ServerAPI.Public";
        service.AddHttpClient<IPublicService, PublicService>(MRPublicDataService, client => client.BaseAddress = baseAddress);

        return service;
    }
}
