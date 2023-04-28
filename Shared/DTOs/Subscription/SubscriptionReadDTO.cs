using MR.Shared.DTOs.Payment;

namespace MR.Shared.DTOs.Subscription;

public class SubscriptionReadDTO
{
    public decimal Price { get; set; } = 0;
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public PaymentDTO? LastPayment { get; set; }
    public bool IsActive { get; set; }
}