namespace Quorum.Client.Services;

public interface IAdminService
{
    Task<ApiResponse<AdminLogPagedListDTO>> GetAdminLogs(AdminLogSearchParamsDTO query);
    Task<ApiResponse<PaymentPagedListDTO>> SeedPayments(SeedPaymentRequest seedPaymentRequest);
    Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeActivate();
    Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeDeactivate();
    Task<ApiResponse<SubscriptionPagedListDTO>> ActivateSubscriptions();
    Task<ApiResponse<bool>> ActivateSubscription(string applicationUserId);
    Task<ApiResponse<bool>> DeactivateSubscription(string applicationUserId);
    Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsBySearchParams(SubscriptionSearchParamsDTO query);
    Task<ApiResponse<string>> CreateOrEditSubscription(SubscriptionCreateForUserDTO SubscriptionDto);
    Task<ApiResponse<Guid>> InitQuarter(InitQuarterDTO quarter);
    Task<ApiResponse<PagedListDto<QuarterDTO>>> GetQuarters(QuarterSearchParamsDTO searchParams);
    Task<ApiResponse<PagedListDto<SignaturePoolAdminDTO>>> GetSignaturePoolsBySearchParams(SignaturePoolAdminSearchParamsDTO searchParams);
    Task<ApiResponse<bool>> UnpinSignatureFromIssue(Guid signatureId);
    Task<ApiResponse<bool>> RemoveSignature(Guid signatureId);
    Task<ApiResponse<bool>> AddSignatureToSignaturePool(Guid signaturePoolId);
    Task<ApiResponse<PagedListDto<IssueReadDTO>>> GetIssuesBySearchParams(IssueSearchParamsDTO searchParams);
    Task<ApiResponse<Guid>> CreateIssue(IssueAdminCreateDTO issueDTO);
    Task<ApiResponse<string>> GetUserEmailByUserId(string userId);
    Task<ApiResponse<bool>> VerifyIssue(Guid issueId);
    Task<ApiResponse<bool>> ForceDeleteIssue(Guid issueId);
    Task<ApiResponse<bool>> DeleteQuarter(Guid issueId);
    Task<ApiResponse<PagedListDto<IssueAdminRatingValueCalculate>>> CalculatePublishedIssueRatingForCurrentQuarter();
    Task<ApiResponse<IssueReadDTO>> ChooseTheWinnerOfCurrentQuarter();
    Task<ApiResponse<string>> GeneratePDFForAnIssue(Guid issueId);
}

internal sealed class AdminService : DataServiceBase, IAdminService
{
    private const string _adminControllerPath = $"{_apiVersion}Admin";
    private const string _adminSubscriptionControllerPath = $"{_apiVersion}AdminSubscription";
    private const string _adminIssueControllerPath = $"{_apiVersion}AdminIssue";
    private const string _adminQuarterControllerPath = $"{_apiVersion}AdminQuarter";
    private const string _adminAdminSignaturePoolControllerPath = $"{_apiVersion}AdminSignaturePool";

    public AdminService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<AdminLogPagedListDTO>> GetAdminLogs(AdminLogSearchParamsDTO query)
    {
        var q = BuildQuery(query);
        var endpoint = $"{_adminControllerPath}/get-admin-logs-by-query?{q}";
        return await HandleResponse<AdminLogPagedListDTO>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<PaymentPagedListDTO>> SeedPayments(SeedPaymentRequest seedPaymentRequest)
    {
        var endpoint = $"{_adminControllerPath}/seed-payments";
        return await HandleResponse<PaymentPagedListDTO>(async () => await _httpClient.PostAsJsonAsync(endpoint, seedPaymentRequest));
    }

    public async Task<ApiResponse<string>> CreateOrEditSubscription(SubscriptionCreateForUserDTO SubscriptionDto)
    {
        var endpoint = $"{_adminSubscriptionControllerPath}/create-or-edit-subscription";
        return await HandleResponse<string>(async () => await _httpClient.PostAsJsonAsync(endpoint, SubscriptionDto));
    }

    public async Task<ApiResponse<Guid>> InitQuarter(InitQuarterDTO quarter)
    {
        var endpoint = $"{_adminQuarterControllerPath}/init-quarter";
        return await HandleResponse<Guid>(async () => await _httpClient.PostAsJsonAsync(endpoint, quarter));
    }

    public async Task<ApiResponse<PagedListDto<QuarterDTO>>> GetQuarters(QuarterSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_adminQuarterControllerPath}/get-quarters-by-search-params?{q}";
        return await HandleResponse<PagedListDto<QuarterDTO>>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<PagedListDto<SignaturePoolAdminDTO>>> GetSignaturePoolsBySearchParams(SignaturePoolAdminSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_adminAdminSignaturePoolControllerPath}/get-signature-pools-by-search-params?{q}";
        return await HandleResponse<PagedListDto<SignaturePoolAdminDTO>>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> UnpinSignatureFromIssue(Guid signatureId)
    {
        var endpoint = $"{_adminAdminSignaturePoolControllerPath}/unpin-signature-from-issue";
        return await HandleResponse<bool>(async () => await _httpClient.PostAsJsonAsync(endpoint, signatureId));
    }

    public async Task<ApiResponse<bool>> RemoveSignature(Guid signatureId)
    {
        var endpoint = $"{_adminAdminSignaturePoolControllerPath}/remove-signature?signatureId={signatureId}";
        return await HandleResponse<bool>(async () => await _httpClient.DeleteAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> AddSignatureToSignaturePool(Guid signaturePoolId)
    {
        var endpoint = $"{_adminAdminSignaturePoolControllerPath}/add-signature-to-signature-pool";
        return await HandleResponse<bool>(async () => await _httpClient.PostAsJsonAsync(endpoint, signaturePoolId));
    }

    public async Task<ApiResponse<PagedListDto<IssueReadDTO>>> GetIssuesBySearchParams(IssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_adminIssueControllerPath}/get-issues-by-search-params-admin?{q}";
        return await HandleResponse<PagedListDto<IssueReadDTO>>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<Guid>> CreateIssue(IssueAdminCreateDTO issueDTO)
    {
        var endpoint = $"{_adminIssueControllerPath}/create-issue-by-admin";
        return await HandleResponse<Guid>(async () => await _httpClient.PostAsJsonAsync(endpoint, issueDTO));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeActivate()
    {
        var endpoint = $"{_adminSubscriptionControllerPath}/get-subscriptions-that-could-be-activated";
        return await HandleResponse<SubscriptionPagedListDTO>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsThatCouldBeDeactivate()
    {
        var endpoint = $"{_adminSubscriptionControllerPath}/get-subscriptions-that-could-be-deactivated";
        return await HandleResponse<SubscriptionPagedListDTO>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> ActivateSubscriptions()
    {
        var endpoint = $"{_adminSubscriptionControllerPath}/activate-subscriptions";
        return await HandleResponse<SubscriptionPagedListDTO>(async () => await _httpClient.PostAsync(endpoint, null));
    }

    public async Task<ApiResponse<bool>> DeactivateSubscription(string applicationUserId)
    {
        var endpoint = $"{_adminSubscriptionControllerPath}/deactivate-subscription/{applicationUserId}";
        return await HandleResponse<bool>(async () => await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<ApiResponse<bool>> ActivateSubscription(string applicationUserId)
    {
        var endpoint = $"{_adminSubscriptionControllerPath}/activate-subscription/{applicationUserId}";
        return await HandleResponse<bool>(async () => await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<ApiResponse<SubscriptionPagedListDTO>> GetSubscriptionsBySearchParams(SubscriptionSearchParamsDTO query)
    {
        var q = BuildQuery(query);
        var endpoint = $"{_adminSubscriptionControllerPath}/get-subscriptions-by-search-params?{q}";
        return await HandleResponse<SubscriptionPagedListDTO>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<string>> GetUserEmailByUserId(string userId)
    {
        var q = BuildQuery(nameof(userId), userId);
        var endpoint = $"{_adminControllerPath}/get-user-email-by-user-id?{q}";
        return await HandleResponse<string>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> VerifyIssue(Guid issueId)
    {
        var endpoint = $"{_adminIssueControllerPath}/verify-issue/{issueId}";
        return await HandleResponse<bool>(async () => await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<ApiResponse<bool>> ForceDeleteIssue(Guid issueId)
    {
        var endpoint = $"{_adminIssueControllerPath}/force-delete-issue/{issueId}";
        return await HandleResponse<bool>(async () => await _httpClient.DeleteAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> DeleteQuarter(Guid quarterId)
    {
        var endpoint = $"{_adminQuarterControllerPath}/delete-quarter/{quarterId}";
        return await HandleResponse<bool>(async () => await _httpClient.DeleteAsync(endpoint));
    }

    public async Task<ApiResponse<PagedListDto<IssueAdminRatingValueCalculate>>> CalculatePublishedIssueRatingForCurrentQuarter()
    {
        var endpoint = $"{_adminIssueControllerPath}/calculate-rating-for-published-issues";
        return await HandleResponse<PagedListDto<IssueAdminRatingValueCalculate>>(async () => await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<ApiResponse<IssueReadDTO>> ChooseTheWinnerOfCurrentQuarter()
    {
        var endpoint = $"{_adminIssueControllerPath}/choose-the-winner-of-current-quarter";
        return await HandleResponse<IssueReadDTO>(async () => await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<ApiResponse<string>> GeneratePDFForAnIssue(Guid issueId)
    {
        var endpoint = $"{_adminIssueControllerPath}/generate-pdf-for-an-issue/{issueId}";
        return await HandleResponse<string>(async () => await _httpClient.PutAsync(endpoint, null));
    }
}
