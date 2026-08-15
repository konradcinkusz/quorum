namespace Quorum.Shared.DTOs.Subscription;

public class SubscriptionReadDTO
{
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public PaymentStatusEnum? PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool IsActive { get; set; }
    public SubscriptionViewStatusEnum SubscriptionViewStatusEnum { get; set; } = SubscriptionViewStatusEnum.YouDontHaveActiveSub;
}
