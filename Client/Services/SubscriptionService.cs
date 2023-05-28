namespace MR.Client.Services;

public interface ISubscriptionService
{
    Task<ApiResponse<SubscriptionReadDTO>> GetSubscription();
    Task<string> Buy();
    Task<PaymentPagedListDTO> GetMyPayments(PaymentSearchParamsDTO query);
}

internal class SubscriptionService : DataServiceBase, ISubscriptionService
{
    public SubscriptionService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<string> Buy()
    {
        var response = await _httpClient.PostAsync($"{_subscriptionControllerPath}", null);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

        return apiResponse.Message.ToString();
    }

    public async Task<ApiResponse<SubscriptionReadDTO>> GetSubscription()
    {
        var endpoint = $"{_subscriptionControllerPath}/get-subscription";
        return await HandleResponse<SubscriptionReadDTO>(async () =>
                    await _httpClient.GetAsync(endpoint));
    }

    public async Task<PaymentPagedListDTO> GetMyPayments(PaymentSearchParamsDTO query)
    {
        var q = BuildQuery(query);

        var response = await _httpClient.GetAsync($"{_subscriptionControllerPath}/GetMyPayments?{q}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<PaymentPagedListDTO>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? throw new Exception("Deserialized response is null.");
    }
}
