namespace MR.Client.Services;

public interface IAdminService
{
    Task<AdminLogPagedListDTO> GetAdminLogs(AdminLogSearchParamsDTO query);
    Task<bool> SeedPayments(SeedPaymentRequest seedPaymentRequest);
    Task<int> ActivateSubscription();
    Task<string> CreateSubscription(SubscriptionCreateForUserDTO SubscriptionDto);
    Task<SubscriptionPagedListDTO> GetSubscriptions(SubscriptionSearchParamsDTO query);
    Task<ApiResponse<Guid>> InitQuarter(InitQuarterDTO quarter);
    Task<ApiResponse<QuarterPagedListDTO>> GetQuarters(QuarterSearchParamsDTO searchParams);
    Task<ApiResponse<SignaturePoolsPagedListDTO>> GetSignaturePools(SignaturePoolsSearchParamsDTO searchParams);
}

internal class AdminService : DataServiceBase, IAdminService
{
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
        var response = await _httpClient.PostAsJsonAsync(
            $"{_subscriptionControllerPath}/CreateSubscriptionForUser", SubscriptionDto);

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

    public async Task<int> ActivateSubscription()
    {
        var response = await _httpClient.PostAsync($"{_adminControllerPath}/ActivateSubscription",
                                                       null);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<int>();
        return result;
    }

    public async Task<ApiResponse<Guid>> InitQuarter(InitQuarterDTO quarter)
    {
        var endpoint = $"{_QuarterControllerPath}/InitQuarter";

        var response = await _httpClient.PostAsJsonAsync(endpoint, quarter);

        if (!response.IsSuccessStatusCode)
        {
            // Handle error response
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();

        if (apiResponse == null)
        {
            apiResponse = new ApiResponse<Guid> { Data = Guid.Empty, Message = "The response is empty" };
        }

        return apiResponse;
    }

    public async Task<ApiResponse<QuarterPagedListDTO>> GetQuarters(QuarterSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);

        if (searchParams.Begin.HasValue)
            q[nameof(searchParams.Begin)] = searchParams.Begin.Value.ToString();
        if (searchParams.End.HasValue)
            q[nameof(searchParams.End)] = searchParams.End.Value.ToString();
        if (searchParams.Year.HasValue)
            q[nameof(searchParams.Year)] = searchParams.Year.Value.ToString();
        if (searchParams.Quarter.HasValue)
            q[nameof(searchParams.Quarter)] = searchParams.Quarter.Value.ToString();

        var response = await _httpClient.GetAsync($"{_QuarterControllerPath}/GetQuartersBySearchParams?{q}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ApiResponse<QuarterPagedListDTO>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? throw new Exception("Deserialized response is null.");
    }

    public async Task<ApiResponse<SignaturePoolsPagedListDTO>> GetSignaturePools(SignaturePoolsSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);

        if (searchParams.Begin.HasValue)
            q[nameof(searchParams.Begin)] = searchParams.Begin.Value.ToString();
        if (searchParams.End.HasValue)
            q[nameof(searchParams.End)] = searchParams.End.Value.ToString();
        if (searchParams.Year.HasValue)
            q[nameof(searchParams.Year)] = searchParams.Year.Value.ToString();
        if (searchParams.Quarter.HasValue)
            q[nameof(searchParams.Quarter)] = searchParams.Quarter.Value.ToString();

        var response = await _httpClient.GetAsync($"{_SignaturePoolControllerPath}/GetSignaturePoolsBySearchParams?{q}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ApiResponse<SignaturePoolsPagedListDTO>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? throw new Exception("Deserialized response is null.");
    }
}
