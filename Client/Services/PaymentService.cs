namespace MR.Client.Services;

public class PaymentService : DataServiceBase, IPaymentService
{
    private const string _paymentControllerPath = @"/api/v1.0/Payment";

    public PaymentService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<string> CreatePayment(PaymentCreateDTO paymentDto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_paymentControllerPath}/CreatePayment", paymentDto);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
    public async Task<PaymentDTO> GetPayment(Guid id)
    {
        var response = await _httpClient.GetAsync($"{_paymentControllerPath}/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var paymentDto = await response.Content.ReadFromJsonAsync<PaymentDTO>();

        return paymentDto;
    }

    public async Task<PaymentPagedListDto> GetPayments(PaymentSearchParamsDTO query)
    {
        var q = BuildQuery(query);

        var response = await _httpClient.GetAsync($"{_paymentControllerPath}/GetPaymentsByQuery?{q}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<PaymentPagedListDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? throw new Exception("Deserialized response is null.");
    }

    public async Task<bool> SeedPayments()
    {
        var result = await _httpClient.PostAsync($"{_paymentControllerPath}/SeedPayments",
                                                       null);
        return result.IsSuccessStatusCode;
    }

    public Task UpdatePayment(PaymentUpdateDTO paymentUpdateDTO)
    {
        throw new NotImplementedException();
    }
}
