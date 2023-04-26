namespace MR.Client.Services;

public abstract class DataServiceBase
{
    protected readonly HttpClient _httpClient;

    public DataServiceBase(HttpClient httpclient) => _httpClient = httpclient;

    public string GetBaseUrl() => _httpClient.BaseAddress.ToString();

    protected NameValueCollection BuildQuery(SearchParams query)
    {
        var q = HttpUtility.ParseQueryString(string.Empty);
        q[nameof(query.CurrentPage)] = query.CurrentPage.ToString();
        q[nameof(query.PageSize)] = query.PageSize.ToString();
        return q;
    }
}
