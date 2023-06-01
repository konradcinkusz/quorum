namespace MR.Client.Services;

public interface IIssueService
{
    Task<ApiResponse<Guid>> CreateOrEditIssue(IssueCreateDTO issueDTO);
    Task<ApiResponse<IssuePagedListDTO>> GetIssuesBySearchParams(IssueSearchParamsDTO searchParams);
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

    public async Task<ApiResponse<IssuePagedListDTO>> GetIssuesBySearchParams
        (IssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_issueControllerPath}/get-issues-by-search-params?{q}";
        return await HandleResponse<IssuePagedListDTO>(async () =>
        await _httpClient.GetAsync(endpoint));
    }
}
