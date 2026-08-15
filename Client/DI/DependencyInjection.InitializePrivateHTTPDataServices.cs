using Quorum.Client.Features.Auth;

namespace Quorum.Client.DI;

public static partial class DependencyInjection
{
    /// <summary>
    /// The authenticated data services. No token handler attaches anything here any more:
    /// the browser sends the BFF's HttpOnly cookie with every same-origin request, and the
    /// server's cookie bridge turns it into the bearer header (ADR 0001). The only
    /// auth-aware piece left is <see cref="SessionRefreshHandler"/>, which converts an
    /// access-token expiry into a silent refresh-and-replay instead of a visible 401.
    /// </summary>
    public static IServiceCollection InitializePrivateHTTPDataServices(this IServiceCollection service, Uri baseAddress)
    {
        const string QuorumPaymentDataService = "Quorum.ServerAPI";
        service
            .AddHttpClient<IPaymentService, PaymentService>(QuorumPaymentDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<SessionRefreshHandler>();

        const string QuorumAdminDataService = "Quorum.ServerAPI.Admin";
        service
            .AddHttpClient<IAdminService, AdminService>(QuorumAdminDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<SessionRefreshHandler>();

        const string QuorumSubscriptionDataService = "Quorum.ServerAPI.Subscription";
        service
            .AddHttpClient<ISubscriptionService, SubscriptionService>(QuorumSubscriptionDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<SessionRefreshHandler>();

        const string QuorumIssueDataService = "Quorum.ServerAPI.Issue";
        service
            .AddHttpClient<IIssueService, IssueService>(QuorumIssueDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<SessionRefreshHandler>();

        const string QuorumSignaturePoolDataService = "Quorum.ServerAPI.SignaturePool";
        service
            .AddHttpClient<ISignaturePoolService, SignaturePoolService>(QuorumSignaturePoolDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<SessionRefreshHandler>();

        const string QuorumSignatureDataService = "Quorum.ServerAPI.Signature";
        service
            .AddHttpClient<ISignatureService, SignatureService>(QuorumSignatureDataService, client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<SessionRefreshHandler>();

        service.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        return service;
    }
}
