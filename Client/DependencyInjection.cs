namespace MR.Client;

public static class DependencyInjection
{
    public static IServiceCollection InitializeHTTPDataServices(this IServiceCollection service, Uri baseAddress)
    {
        service.AddTransient<RoleAuthorizationMessageHandler>();

        // Add HttpClient and services here
        const string MRPaymentDataService = "MR.ServerAPI";
        service
            .AddHttpClient<IPaymentService, PaymentService>(MRPaymentDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
            .AddHttpMessageHandler<RoleAuthorizationMessageHandler>();

        const string MRAdmintDataService = "MR.ServerAPI.Admin";
        service
            .AddHttpClient<IAdminService, AdminService>(MRAdmintDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
            .AddHttpMessageHandler<RoleAuthorizationMessageHandler>();

        const string MRSubscriptionDataService = "MR.ServerAPI.Subscription";
        service
            .AddHttpClient<ISubscriptionService, SubscriptionService>(MRSubscriptionDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        const string MRIssueDataService = "MR.ServerAPI.Issue";
        service
            .AddHttpClient<IIssueService, IssueService>(MRIssueDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        // Supply HttpClient instances that include access tokens when making requests to the server project
        //builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(MRPaymentDataService));
        //builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(MRAuthorizedDataService));
        service.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        return service;
    }

}
