namespace MR.Shared.DTOs.Payment;

public class PaymentStatusHistoryDTO
{
    public PaymentStatusEnum PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
