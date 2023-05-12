namespace MR.Client.Services;

internal abstract class DataServiceBase
{
    protected const string _apiVersion = @"/api/v1.0/";
    protected const string _adminControllerPath = $"{_apiVersion}Admin";
    protected const string _subscriptionControllerPath = $"{_apiVersion}Subscription";
    protected const string _QuarterControllerPath = $"{_apiVersion}Quarter";
    protected const string _paymentControllerPath = $"{_apiVersion}Payment";

    protected readonly HttpClient _httpClient;

    public DataServiceBase(HttpClient httpclient) => _httpClient = httpclient;

    public string GetBaseUrl() => _httpClient.BaseAddress.ToString();

    protected NameValueCollection BuildQuery(SearchParamsDTO query)
    {
        var q = HttpUtility.ParseQueryString(string.Empty);
        q[nameof(query.CurrentPage)] = query.CurrentPage.ToString();
        q[nameof(query.PageSize)] = query.PageSize.ToString();
        q[nameof(query.SortOrder)] = query.SortOrder.ToString();
        q[nameof(query.SortColumn)] = query.SortColumn.ToString();
        return q;
    }
}
