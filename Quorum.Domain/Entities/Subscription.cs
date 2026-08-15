namespace Quorum.Domain.Entities;

[Table(nameof(TableNames.Subscriptions), Schema = SchemasNames.MRBasics)]
public class Subscription
{
    /// <summary>Subject id from the identity service; no navigation on purpose (ADR 0001).</summary>
    [Key]
    public string ApplicationUserId { get; set; }

    /// <summary>Display email resolved from the <see cref="QuorumUser"/> projection by the
    /// query layer; not a column, and never authoritative.</summary>
    [NotMapped]
    public string? ApplicationUserEmail { get; set; }

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