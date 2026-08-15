namespace Quorum.Domain.Entities;

[Table(nameof(TableNames.Subscriptions), Schema = SchemasNames.MRBasics)]
public class Subscription
{
    [ForeignKey(nameof(ApplicationUser)), Key]
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive()
    {
        if (Begin == null || End == null)
        {
            return false;
        }

        var currentDate = DateTime.UtcNow;
        return currentDate >= Begin && currentDate <= End;
    }

    [InverseProperty(nameof(SubscriptionPayment.Subscription))]
    public ICollection<SubscriptionPayment> SubscriptionPayments { get; set; }
    [InverseProperty(nameof(Subscription_Log.Subscription))]
    public ICollection<Subscription_Log> Subscription_Logs { get; set; }

}

[Table(nameof(TableNames.Subscription_Logs), Schema = SchemasNames.MRBasics)]
public class Subscription_Log : BaseEntityLog
{
    [ForeignKey(nameof(Subscription))]
    public string SubscriptionId { get; set; }
    public Subscription Subscription { get; set; }
}