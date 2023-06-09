namespace MR.Client.Services;

public interface ISubscriptionService
{
    Task<ApiResponse<SubscriptionReadDTO>> GetMySubscription();
    Task<ApiResponse<bool>> BuySubscription();
    Task<ApiResponse<bool>> RejectSubscription();
    Task<ApiResponse<bool>> UnsubscribeSubscription();
}

internal class SubscriptionService : DataServiceBase, ISubscriptionService
{
    public SubscriptionService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<bool>> BuySubscription()
    {
        var endpoint = $"{_subscriptionControllerPath}/buy-subscription";
        return await HandleResponse<bool>(async () =>
                    await _httpClient.PostAsync(endpoint, null));
    }

    public async Task<ApiResponse<SubscriptionReadDTO>> GetMySubscription()
    {
        var endpoint = $"{_subscriptionControllerPath}/get-my-subscription";
        return await HandleResponse<SubscriptionReadDTO>(async () =>
                    await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> RejectSubscription()
    {
        var endpoint = $"{_subscriptionControllerPath}/reject-subscription";
        return await HandleResponse<bool>(async () =>
                    await _httpClient.PostAsync(endpoint, null));
    }

    public Task<ApiResponse<bool>> UnsubscribeSubscription()
    {
        throw new NotImplementedException();
    }
}
