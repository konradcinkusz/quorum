using MR.Shared.DTOs.Subscription;

namespace MR.Client.Services;

public interface ISubscriptionService
{
    Task<string> CreateSubscription(SubscriptionCreateForUserDTO SubscriptionDto);
    Task<SubscriptionPagedListDTO> GetSubscriptions(SubscriptionSearchParamsDTO query);
}

public class SubscriptionService : DataServiceBase, ISubscriptionService
{
    private const string _subscriptionControllerPath = @"/api/v1.0/Subscription";

    public SubscriptionService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<string> CreateSubscription(SubscriptionCreateForUserDTO SubscriptionDto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_subscriptionControllerPath}/CreateSubscriptionForUser", SubscriptionDto);

        response.EnsureSuccessStatusCode();

        return response.Headers.Location.Segments.Last();
    }

    public async Task<SubscriptionPagedListDTO> GetSubscriptions(SubscriptionSearchParamsDTO query)
    {
        var q = BuildQuery(query);

        if (!string.IsNullOrEmpty(query.ApplicationUserId))
            q[nameof(query.ApplicationUserId)] = query.ApplicationUserId;

        var response = await _httpClient.GetAsync($"{_subscriptionControllerPath}/GetSubscriptionsByQuery?{q}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<SubscriptionPagedListDTO>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? throw new Exception("Deserialized response is null.");
    }

}
