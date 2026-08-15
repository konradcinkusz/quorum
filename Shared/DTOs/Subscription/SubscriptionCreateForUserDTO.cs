namespace Quorum.Shared.DTOs.Subscription;

public class SubscriptionCreateForUserDTO
{
    public string ApplicationUserId { get; set; }

    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
}