namespace MR.Shared.DTOs.Payment;

public class PaymentDTO
{
    public Guid Id { get; set; }
    public string UserEmail { get; set; }
    public string PaymentLink { get; set; }
    public string ClientReferenceId { get; set; }
    public string PaymentIntentId { get; set; }
    public string SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal PaymentValuePLN { get; set; }
    public List<PaymentStatusHistoryDTO> PaymentStatusHistory { get; set; }
}
