namespace MR.Client.Services;

public interface IIssueService
{
    Task<ApiResponse<Guid>> CreateOrEditIssue(IssueCreateDTO issueDTO);
    Task<ApiResponse<IssuePagedListDTO>> GetIssuesBySearchParams(IssueSearchParamsDTO searchParams);
    Task<ApiResponse<IssuePagedListDTO>> GetMyIssuesBySearchParams(IssueSearchParamsDTO searchParams);
    Task<ApiResponse<bool>> PublishIssue(Guid issueId);
    Task<ApiResponse<bool>> PayForAnIssue(Guid issueId, IssuePayDTO issuePayDTO);
}

internal class IssueService : DataServiceBase, IIssueService
{
    public IssueService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<Guid>> CreateOrEditIssue(IssueCreateDTO issueDTO)
    {
        var endpoint = $"{_issueControllerPath}/create-issue";
        return await HandleResponse<Guid>(
            async () => await _httpClient.PostAsJsonAsync(endpoint, issueDTO));
    }

    public async Task<ApiResponse<bool>> PublishIssue(Guid issueId)
    {
        var endpoint = $"{_issueControllerPath}/publish-issue/{issueId}";
        return await HandleResponse<bool>(
            async () => await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<ApiResponse<IssuePagedListDTO>> GetIssuesBySearchParams
        (IssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_issueControllerPath}/get-issues-by-search-params?{q}";
        return await HandleResponse<IssuePagedListDTO>(
            async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> PayForAnIssue(Guid issueId, IssuePayDTO issuePayDTO)
    {
        var endpoint = $"{_issueControllerPath}/pay-for-an-issue/{issueId}";
        return await HandleResponse<bool>(
            async () => await _httpClient.PutAsJsonAsync(endpoint, issuePayDTO));
    }
    public async Task<ApiResponse<IssuePagedListDTO>> GetMyIssuesBySearchParams
        (IssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_issueControllerPath}/get-my-issues-by-search-params?{q}";
        return await HandleResponse<IssuePagedListDTO>(
            async () => await _httpClient.GetAsync(endpoint));
    }
}
