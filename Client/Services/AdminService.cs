namespace MR.Client.Services;

public interface IAdminService
{
    Task<AdminLogPagedListDTO> GetAdminLogs(AdminLogSearchParamsDTO query);
    Task<bool> SeedPayments(SeedPaymentRequest seedPaymentRequest);
    Task<string> CreateSubscription(SubscriptionCreateForUserDTO SubscriptionDto);
    Task<SubscriptionPagedListDTO> GetSubscriptions(SubscriptionSearchParamsDTO query);
}

public class AdminService : DataServiceBase, IAdminService
{
    private const string _adminControllerPath = @"/api/v1.0/Admin";

    private const string _subscriptionControllerPath = @"/api/v1.0/Subscription";

    public AdminService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<AdminLogPagedListDTO> GetAdminLogs(AdminLogSearchParamsDTO query)
    {
        var q = BuildQuery(query);

        if (!string.IsNullOrEmpty(query.ValuesText))
            q[nameof(query.ValuesText)] = query.ValuesText;

        if (!string.IsNullOrEmpty(query.Action))
            q[nameof(query.Action)] = query.Action;

        q[nameof(query.LastHour)] = query.LastHour.ToString();
        q[nameof(query.LastMonth)] = query.LastMonth.ToString();

        var response = await _httpClient.GetAsync($"{_adminControllerPath}/GetAdminLogsByQuery?{q}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<AdminLogPagedListDTO>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? throw new Exception("Deserialized response is null.");
    }

    public async Task<bool> SeedPayments(SeedPaymentRequest seedPaymentRequest)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_adminControllerPath}/SeedPayments",
                                                       seedPaymentRequest);
        return result.IsSuccessStatusCode;
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
