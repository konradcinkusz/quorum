namespace MR.Shared.DTOs.Payment;

public class PaymentSearchParamsDTO : SearchParamsDTO
{
    public string? PaymentId { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
}

