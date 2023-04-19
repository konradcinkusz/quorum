namespace MR.Client.Services;

public interface IPaymentService
{
    Task<string> CreatePayment(PaymentCreateDTO paymentDto);
    Task<PaymentReadDTO> GetPayment(Guid id);
    Task<PaymentPagedListDto> GetPayments(PaymentSearchParamsDTO query);
    Task<bool> SeedPayments();
}
