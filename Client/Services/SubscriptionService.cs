namespace MR.Client.Services;

public interface ISubscriptionService
{
    Task<SubscriptionReadDTO> Get();
    Task Buy();
}

public class SubscriptionService : DataServiceBase, ISubscriptionService
{
    private const string _subscriptionControllerPath = @"/api/v1.0/Subscription";

    public SubscriptionService(HttpClient httpclient) : base(httpclient)
    {
    }

    public Task Buy()
    {
        throw new NotImplementedException();
    }

    public async Task<SubscriptionReadDTO> Get()
    {
        var response = await _httpClient.GetAsync(_subscriptionControllerPath);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var paymentDto = await response.Content.ReadFromJsonAsync<SubscriptionReadDTO>();

        return paymentDto;
    }
}
