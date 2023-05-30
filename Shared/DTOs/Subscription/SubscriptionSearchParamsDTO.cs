namespace MR.Shared.DTOs.Subscription;

public class SubscriptionSearchParamsDTO : SearchParamsDTO
{
    public string? Id { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public SubscriptionActivityEnum? Activity { get; set; }
}
