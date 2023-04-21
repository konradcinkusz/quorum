namespace MR.Client.Services;

public interface IAdminService
{
    Task<AdminLogPagedListDTO> GetAdminLogs(AdminLogSearchParamsDTO query);
    Task<bool> SeedPayments();
}

public class AdminService : DataServiceBase, IAdminService
{
    private const string _adminControllerPath = @"/api/v1.0/Admin";

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

    public async Task<bool> SeedPayments()
    {
        var result = await _httpClient.PostAsync($"{_adminControllerPath}/SeedPayments",
                                                       null);
        return result.IsSuccessStatusCode;
    }
}
