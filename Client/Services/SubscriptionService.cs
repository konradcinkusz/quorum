namespace MR.Client.Services;

public interface ISubscriptionService
{
    Task<ApiResponse<SubscriptionReadDTO>> GetSubscription();
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

    public async Task<ApiResponse<SubscriptionReadDTO>> GetSubscription()
    {
        var endpoint = $"{_subscriptionControllerPath}/get-subscription";
        return await HandleResponse<SubscriptionReadDTO>(async () =>
                    await _httpClient.GetAsync(endpoint));
    }

    public Task<ApiResponse<bool>> RejectSubscription()
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> UnsubscribeSubscription()
    {
        throw new NotImplementedException();
    }
}
