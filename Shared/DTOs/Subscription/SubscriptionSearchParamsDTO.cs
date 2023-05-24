namespace MR.Shared.DTOs.Subscription;

public class SubscriptionSearchParamsDTO : SearchParamsDTO
{
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public string? ApplicationUserEmail { get; set; }
}
