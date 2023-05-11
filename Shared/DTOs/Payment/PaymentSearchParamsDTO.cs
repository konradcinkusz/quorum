namespace MR.Shared.DTOs.Payment;

public class PaymentSearchParamsDTO : SearchParamsDTO
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
    public string Description { get; set; } = string.Empty;
}

