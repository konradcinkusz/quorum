namespace MR.Client.Services;

internal abstract class DataServiceBase
{
    protected const string _apiVersion = @"/api/v1.0/";
    protected const string _subscriptionControllerPath = $"{_apiVersion}Subscription";
    protected const string _signaturePoolControllerPath = $"{_apiVersion}SignaturePool";
    protected const string _paymentControllerPath = $"{_apiVersion}Payment";
    protected const string _issueControllerPath = $"{_apiVersion}Issue";
    protected const string _signatureControllerPath = $"{_apiVersion}Signature";

    protected readonly HttpClient _httpClient;

    public DataServiceBase(HttpClient httpclient) => _httpClient = httpclient;

    public string GetBaseUrl() => _httpClient.BaseAddress.ToString();

    protected NameValueCollection BuildQuery<T>(T query) where T : SearchParamsDTO
    {
        var q = HttpUtility.ParseQueryString(string.Empty);

        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(query);
            if (value != null)
            {
                if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    var dateTimeValue = value as DateTime?;
                    var formattedValue = dateTimeValue?.ToString("yyyy-MM-dd");
                    q[property.Name] = formattedValue;
                }
                else
                {
                    q[property.Name] = value.ToString();
                }
            }
        }

        return q;
    }

    protected NameValueCollection BuildQuery(Dictionary<string, string> parameters)
    {
        var q = HttpUtility.ParseQueryString(string.Empty);
        foreach (var parameter in parameters)
        {
            q[parameter.Key] = parameter.Value;
        }
        return q;
    }

    protected NameValueCollection BuildQuery(string parameterName, string parameterValue)
    {
        return BuildQuery(new Dictionary<string, string>
                {
                    { parameterName, parameterValue },
                });
    }

    protected async Task<ApiResponse<T>> HandleResponse<T>(Func<Task<HttpResponseMessage>> action)
    {
        var response = await action.Invoke();

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                // Handle 400 Bad Request error
                var errorMessage = await response.Content.ReadAsStringAsync();
                return new ApiResponse<T>(errorMessage, (int)response.StatusCode);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // Handle 404 Not Found error
                return new ApiResponse<T>("Resource not found", (int)response.StatusCode);
            }
            else
            {
                // Handle other error status codes
                return new ApiResponse<T>("An error occurred", (int)response.StatusCode);
            }
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();

        if (apiResponse == null)
        {
            apiResponse = new ApiResponse<T> { Message = "The response is empty" };
        }

        return apiResponse;
    }
}
