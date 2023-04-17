namespace MR.Shared.DTOs.Payment;

public class PaymentQueryDTO
{
    public string UserEmail { get; set; }
    public string ClientReferenceId { get; set; }
    public string PaymentIntentId { get; set; }
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
}
