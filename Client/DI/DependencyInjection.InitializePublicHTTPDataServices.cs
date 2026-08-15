namespace Quorum.Client.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection InitializePublicHTTPDataServices(this IServiceCollection service, Uri baseAddress)
    {
        const string QuorumPublicDataService = "Quorum.ServerAPI.Public";
        service.AddHttpClient<IPublicService, PublicService>(QuorumPublicDataService, client => client.BaseAddress = baseAddress);

        return service;
    }
}
