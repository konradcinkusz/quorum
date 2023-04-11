using MR.Shared.ViewModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MR.Client.Services;

public class PaymentService
{
    private readonly HttpClient _httpClient;

    public PaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentViewModel> GetPayment(int id)
    {
        return await _httpClient.GetFromJsonAsync<PaymentViewModel>($"/api/payments/{id}");
    }

    public async Task<IEnumerable<PaymentViewModel>> GetPayments()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<PaymentViewModel>>("/api/payments");
    }
}
