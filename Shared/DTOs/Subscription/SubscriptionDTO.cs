namespace MR.Shared.DTOs.Subscription;

public class SubscriptionDTO
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; }

    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public bool IsActive { get; set; }
}
