namespace MR.Client.Services;

public interface IIssueService
{
    Task<ApiResponse<Guid>> EditIssue(Guid issueId, IssueCreateDTO issueDTO);
    Task<ApiResponse<Guid>> CreateIssue(IssueCreateDTO issueDTO);
    Task<ApiResponse<Guid>> ChangeIssueProcessStatus(Guid issueId, IssueProcessEnum newIssueProcessStatus);
    Task<ApiResponse<PagedListDto<IssueReadDTO>>> GetIssuesBySearchParams(IssueSearchParamsDTO searchParams);
    Task<ApiResponse<PagedListDto<IssueReadDTO>>> GetMyIssuesBySearchParams(IssueSearchParamsDTO searchParams);
    Task<ApiResponse<bool>> PublishIssue(Guid issueId);
    Task<ApiResponse<bool>> PayForAnIssue(Guid issueId, IssuePayDTO issuePayDTO);
}

internal class IssueService : DataServiceBase, IIssueService
{
    public IssueService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<Guid>> CreateIssue(IssueCreateDTO issueDTO)
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

    public async Task<ApiResponse<PagedListDto<IssueReadDTO>>> GetIssuesBySearchParams
        (IssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_issueControllerPath}/get-issues-by-search-params?{q}";
        return await HandleResponse<PagedListDto<IssueReadDTO>>(
            async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<bool>> PayForAnIssue(Guid issueId, IssuePayDTO issuePayDTO)
    {
        var endpoint = $"{_issueControllerPath}/pay-for-an-issue/{issueId}";
        return await HandleResponse<bool>(
            async () => await _httpClient.PutAsJsonAsync(endpoint, issuePayDTO));
    }
    public async Task<ApiResponse<PagedListDto<IssueReadDTO>>> GetMyIssuesBySearchParams
        (IssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_issueControllerPath}/get-my-issues-by-search-params?{q}";
        return await HandleResponse<PagedListDto<IssueReadDTO>>(
            async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<ApiResponse<Guid>> ChangeIssueProcessStatus(Guid issueId, IssueProcessEnum newIssueProcessStatus)
    {
        var endpoint = $"{_issueControllerPath}/change-issue-process-status/{issueId}";
        return await HandleResponse<Guid>(
            async () => await _httpClient.PutAsJsonAsync(endpoint, newIssueProcessStatus));
    }

    public async Task<ApiResponse<Guid>> EditIssue(Guid issueId, IssueCreateDTO issueDTO)
    {
        var endpoint = $"{_issueControllerPath}/edit-issue/{issueId}";
        return await HandleResponse<Guid>(
            async () => await _httpClient.PutAsJsonAsync(endpoint, issueDTO));
    }
}
