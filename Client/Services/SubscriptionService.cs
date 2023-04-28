namespace MR.Client.Services;

public interface ISubscriptionService
{
    Task<SubscriptionReadDTO> Get();
    Task<string> Buy();
    Task<PaymentPagedListDTO> GetMyPayments(PaymentSearchParamsDTO query);
}

public class SubscriptionService : DataServiceBase, ISubscriptionService
{
    private const string _subscriptionControllerPath = @"/api/v1.0/Subscription";

    public SubscriptionService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<string> Buy()
    {
        var response = await _httpClient.PostAsync($"{_subscriptionControllerPath}", null);

        response.EnsureSuccessStatusCode();

        return response.Headers.Location.Segments.Last();
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
