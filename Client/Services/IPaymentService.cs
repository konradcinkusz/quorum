namespace MR.Client.Services;

public interface IPaymentService
{
    Task<string> CreatePayment(PaymentCreateDTO paymentDto);
    Task<PaymentDTO> GetPayment(Guid id);
    Task<string> UpdatePayment(PaymentUpdateDTO paymentUpdateDTO);
    Task<PaymentPagedListDto> GetPayments(PaymentSearchParamsDTO query);
    Task<bool> SeedPayments();
}
