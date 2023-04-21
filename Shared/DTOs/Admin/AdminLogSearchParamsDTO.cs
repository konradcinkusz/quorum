namespace MR.Shared.DTOs.Payment;

public class AdminLogSearchParamsDTO : SearchParams
{
    public string UserEmail { get; set; } = string.Empty;
    public string ClientReferenceId { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
}
