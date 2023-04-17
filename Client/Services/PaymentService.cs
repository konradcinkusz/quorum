namespace MR.Client.Services;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;

    public PaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> CreatePayment(PaymentCreateDTO paymentDto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1.0/Payment", paymentDto);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
    public async Task<PaymentReadDTO> GetPayment(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/v1.0/Payment/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var paymentDto = await response.Content.ReadFromJsonAsync<PaymentReadDTO>();

        return paymentDto;
    }

    public async Task<IEnumerable<PaymentReadDTO>> GetPayments(PaymentQueryDTO query)
    {
        var response = await _httpClient.GetAsync($"/api/v1.0/Payment?userEmail={query.UserEmail}&clientReferenceId={query.ClientReferenceId}&paymentIntentId={query.PaymentIntentId}&minPaymentValuePLN={query.MinPaymentValuePLN}&maxPaymentValuePLN={query.MaxPaymentValuePLN}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var paymentDTOs = await response.Content.ReadFromJsonAsync<List<PaymentReadDTO>>();

        return paymentDTOs;
    }
}
