namespace MR.Client.Services;

public interface IAdminService
{
    Task<ApiResponse<AdminLogPagedListDTO>> GetAdminLogs(AdminLogSearchParamsDTO query);
    Task<ApiResponse<PaymentPagedListDTO>> SeedPayments(SeedPaymentRequest seedPaymentRequest);
    Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeActivate();
    Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeDeactivate();
    Task<ApiResponse<SubscriptionPagedListDTO>> ActivateSubscription();
    Task<ApiResponse<SubscriptionPagedListDTO>> DeactivateSubscription(string applicationUserId);
    Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsBySearchParams(SubscriptionSearchParamsDTO query);
    Task<ApiResponse<string>> CreateOrEditSubscription(SubscriptionCreateForUserDTO SubscriptionDto);
    Task<ApiResponse<Guid>> InitQuarter(InitQuarterDTO quarter);
    Task<ApiResponse<QuarterPagedListDTO>> GetQuarters(QuarterSearchParamsDTO searchParams);
    Task<ApiResponse<SignaturePoolsPagedListDTO>> GetSignaturePoolsBySearchParams(SignaturePoolsSearchParamsDTO searchParams);
    Task<ApiResponse<bool>> UnpinSignatureFromIssue(Guid signatureId);
    Task<ApiResponse<bool>> RemoveSignature(Guid signatureId);
    Task<ApiResponse<bool>> AddSignatureToSignaturePool(Guid signaturePoolId);
    Task<ApiResponse<IssuePagedListDTO>> GetIssuesBySearchParams(IssueSearchParamsDTO searchParams);
    Task<ApiResponse<Guid>> CreateOrEditIssue(IssueAdminCreateDTO issueDTO);
    Task<ApiResponse<string>> GetUserEmailByUserId(string userId);
}

internal class AdminService : DataServiceBase, IAdminService
{
    public AdminService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<AdminLogPagedListDTO>> GetAdminLogs(AdminLogSearchParamsDTO query)
    {
        var q = BuildQuery(query);
        var endpoint = $"{_adminControllerPath}/get-admin-logs-by-query?{q}";
        return await HandleResponse<AdminLogPagedListDTO>(async () =>
            await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<PaymentPagedListDTO>> SeedPayments(SeedPaymentRequest seedPaymentRequest)
    {
        var endpoint = $"{_adminControllerPath}/seed-payments";
        return await HandleResponse<PaymentPagedListDTO>(async () => await _httpClient.PostAsJsonAsync(endpoint, seedPaymentRequest));
    }

    public async Task<ApiResponse<string>> CreateOrEditSubscription(SubscriptionCreateForUserDTO SubscriptionDto)
    {
        var endpoint = $"{_subscriptionControllerPath}/CreateOrEditSubscription";
        return await HandleResponse<string>(async () =>
            await _httpClient.PostAsJsonAsync(endpoint, SubscriptionDto));
    }

    public async Task<ApiResponse<Guid>> InitQuarter(InitQuarterDTO quarter)
    {
        var endpoint = $"{_quarterControllerPath}/InitQuarter";

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

        var response = await _httpClient.GetAsync($"{_quarterControllerPath}/GetQuartersBySearchParams?{q}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ApiResponse<QuarterPagedListDTO>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? throw new Exception("Deserialized response is null.");
    }

    public async Task<ApiResponse<SignaturePoolsPagedListDTO>> GetSignaturePoolsBySearchParams(SignaturePoolsSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_signaturePoolControllerPath}/get-signature-pools-by-search-params?{q}";
        return await HandleResponse<SignaturePoolsPagedListDTO>(async () =>
        await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> UnpinSignatureFromIssue(Guid signatureId)
    {
        var endpoint = $"{_signaturePoolControllerPath}/unpin-signature-from-issue";
        return await HandleResponse<bool>(async () => await _httpClient.PostAsJsonAsync(endpoint, signatureId));
    }

    public async Task<ApiResponse<bool>> RemoveSignature(Guid signatureId)
    {
        var endpoint = $"{_signaturePoolControllerPath}/remove-signature";
        return await HandleResponse<bool>(async () => await _httpClient.PostAsJsonAsync(endpoint, signatureId));
    }

    public async Task<ApiResponse<bool>> AddSignatureToSignaturePool(Guid signaturePoolId)
    {
        var endpoint = $"{_signaturePoolControllerPath}/add-signature-to-signature-pool";
        return await HandleResponse<bool>(async () => await _httpClient.PostAsJsonAsync(endpoint, signaturePoolId));
    }

    public async Task<ApiResponse<IssuePagedListDTO>> GetIssuesBySearchParams(IssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_issueControllerPath}/get-issues-by-search-params-admin?{q}";
        return await HandleResponse<IssuePagedListDTO>(async () =>
        await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<Guid>> CreateOrEditIssue(IssueAdminCreateDTO issueDTO)
    {
        var endpoint = $"{_issueControllerPath}/create-issue-by-admin";
        return await HandleResponse<Guid>(
            async () => await _httpClient.PostAsJsonAsync(endpoint, issueDTO));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeActivate()
    {
        var endpoint = $"{_subscriptionControllerPath}/get-subscriptions-that-could-be-activated";
        return await HandleResponse<SubscriptionPagedListDTO>(
            async () => await _httpClient.GetAsync(endpoint));
    }
    public async Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeDeactivate()
    {
        var endpoint = $"{_subscriptionControllerPath}/get-subscriptions-that-could-be-deactivated";
        return await HandleResponse<SubscriptionPagedListDTO>(async () =>
        await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> ActivateSubscription()
    {
        var endpoint = $"{_subscriptionControllerPath}/activate-subscription";
        return await HandleResponse<SubscriptionPagedListDTO>(
            async () => await _httpClient.PostAsync(endpoint, null));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> DeactivateSubscription(string applicationUserId)
    {
        var endpoint = $"{_subscriptionControllerPath}/deactivate-subscription";
        return await HandleResponse<SubscriptionPagedListDTO>(
            async () => await _httpClient.PostAsJsonAsync(endpoint, applicationUserId));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsBySearchParams(SubscriptionSearchParamsDTO query)
    {
        var q = BuildQuery(query);
        var endpoint = $"{_subscriptionControllerPath}/get-subscriptions-by-search-params?{q}";
        return await HandleResponse<SubscriptionPagedListDTO>(async () =>
            await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<string>> GetUserEmailByUserId(string userId)
    {
        var q = BuildQuery(nameof(userId), userId);
        var endpoint = $"{_adminControllerPath}/get-user-email-by-user-id?{q}";
        return await HandleResponse<string>(async () =>
            await _httpClient.GetAsync(endpoint));
    }
}
