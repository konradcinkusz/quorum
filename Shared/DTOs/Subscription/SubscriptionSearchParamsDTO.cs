namespace Quorum.Shared.DTOs.Subscription;

public class SubscriptionSearchParamsDTO : SearchParamsDTO
{
    public string? ApplicationUserId { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public SubscriptionActivityEnum? Activity { get; set; }

    public override void Clear()
    {
        this.ApplicationUserId = null;
        this.ApplicationUserEmail = null;
        this.Begin = null;
        this.End = null;
        this.Activity = null;
    }
}
