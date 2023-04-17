namespace MR.Shared.DTOs.Payment;

public class PaymentCreateDTO
{
    public string UserEmail { get; set; }
    public string PaymentLink { get; set; }
    public string ClientReferenceId { get; set; }
    public string PaymentIntentId { get; set; }
    public string SessionId { get; set; }
    public decimal PaymentValuePLN { get; set; }
}
