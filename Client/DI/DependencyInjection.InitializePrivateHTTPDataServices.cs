namespace Quorum.Client.DI;

public static partial class DependencyInjection
{
    public static IServiceCollection InitializePrivateHTTPDataServices(this IServiceCollection service, Uri baseAddress)
    {
        service.AddTransient<RoleAuthorizationMessageHandler>();

        // Add HttpClient and services here
        const string QuorumPaymentDataService = "Quorum.ServerAPI";
        service
            .AddHttpClient<IPaymentService, PaymentService>(QuorumPaymentDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
            .AddHttpMessageHandler<RoleAuthorizationMessageHandler>();

        const string QuorumAdminDataService = "Quorum.ServerAPI.Admin";
        service
            .AddHttpClient<IAdminService, AdminService>(QuorumAdminDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
            .AddHttpMessageHandler<RoleAuthorizationMessageHandler>();

        const string QuorumSubscriptionDataService = "Quorum.ServerAPI.Subscription";
        service
            .AddHttpClient<ISubscriptionService, SubscriptionService>(QuorumSubscriptionDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        const string QuorumIssueDataService = "Quorum.ServerAPI.Issue";
        service
            .AddHttpClient<IIssueService, IssueService>(QuorumIssueDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        const string QuorumSignaturePoolDataService = "Quorum.ServerAPI.SignaturePool";
        service
            .AddHttpClient<ISignaturePoolService, SignaturePoolService>(QuorumSignaturePoolDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        const string QuorumSignatureDataService = "Quorum.ServerAPI.Signature";
        service
            .AddHttpClient<ISignatureService, SignatureService>(QuorumSignatureDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        // Supply HttpClient instances that include access tokens when making requests to the server project
        //builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(QuorumPaymentDataService));
        //builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(MRAuthorizedDataService));
        service.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        return service;
    }
}
