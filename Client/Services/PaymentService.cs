namespace MR.Client.Services;

public interface IPaymentService
{
    Task<string> CreatePayment(PaymentCreateDTO paymentDto);
    Task<PaymentDTO> GetPayment(Guid id);
    Task<string> UpdatePayment(PaymentUpdateDTO paymentUpdateDTO);
    Task<ApiResponse<PagedListDto<PaymentDTO>>> GetPaymentsBySearchParams(PaymentSearchParamsDTO searchParams);
    Task<ApiResponse<bool>> AcceptPayment(Guid paymentId);
}

internal class PaymentService : DataServiceBase, IPaymentService
{
    public PaymentService(HttpClient httpclient) : base(httpclient)
    {
    }

    public async Task<ApiResponse<bool>> AcceptPayment(Guid paymentId)
    {
        var endpoint = $"{_paymentControllerPath}/accept-payment/{paymentId}";
        return await HandleResponse<bool>(async () =>
                    await _httpClient.PutAsync(endpoint, null));
    }

    public async Task<string> CreatePayment(PaymentCreateDTO paymentDto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_paymentControllerPath}/CreatePayment", paymentDto);

        response.EnsureSuccessStatusCode();

        return response.Headers.Location.Segments.Last();
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

    public async Task<ApiResponse<PagedListDto<PaymentDTO>>> GetPaymentsBySearchParams(PaymentSearchParamsDTO query)
    {
        var q = BuildQuery(query);
        var endpoint = $"{_paymentControllerPath}/get-payments-by-search-params?{q}";
        return await HandleResponse<PagedListDto<PaymentDTO>>(async () => await _httpClient.GetAsync(endpoint));
    }

    public async Task<string> UpdatePayment(PaymentUpdateDTO paymentUpdateDTO)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_paymentControllerPath}/edit-payment/{paymentUpdateDTO.Id}", paymentUpdateDTO);
        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException(await response.Content.ReadAsStringAsync());
        }

        return response.Headers.Location.Segments.Last();
    }
}
