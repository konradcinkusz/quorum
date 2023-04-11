namespace MR.Infrastructure.ViewModel;

public class PaymentViewModel
{
    public Guid Id { get; set; }
    public string UserEmail { get; set; }
    public string PaymentLink { get; set; }
    public string ClientReferenceId { get; set; }
    public string PaymentIntentId { get; set; }
    public string SessionId { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal PaymentValuePLN { get; set; }
}
