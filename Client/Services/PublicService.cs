namespace MR.Client.Services;

public interface IPublicService
{
    Task<ApiResponse<PagedListDto<PublicPublishedIssueRead>>> GetMyIssuesBySearchParams(PublicPublishedIssueSearchParamsDTO searchParams);
}

internal class PublicService : DataServiceBase, IPublicService
{
    public PublicService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<PagedListDto<PublicPublishedIssueRead>>> GetMyIssuesBySearchParams(PublicPublishedIssueSearchParamsDTO searchParams)
    {
        var q = BuildQuery(searchParams);
        var endpoint = $"{_issueControllerPath}/get-current-quarter-published?{q}";
        return await HandleResponse<PagedListDto<PublicPublishedIssueRead>>(async () => await _httpClient.GetAsync(endpoint));
    }
}