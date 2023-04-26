namespace MR.Domain.Entities;

[Table(nameof(TableNames.Subscriptions), Schema = SchemasNames.MRBasics)]
public class Subscription : BaseEntity<Guid>
{
    [ForeignKey(nameof(ApplicationUser))]
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public bool IsActive()
    {
        if (Begin == null || End == null)
        {
            return false;
        }

        var currentDate = DateTime.UtcNow;
        return currentDate >= Begin && currentDate <= End;
    }
    [InverseProperty(EntityNames.Subscription)]
    public ICollection<SubscriptionPayment> SubscriptionPayments { get; set; }

}

[Table(nameof(TableNames.Subscription_Logs), Schema = SchemasNames.MRBasics)]
public class Subscription_Log : BaseEntityLog
{
    [ForeignKey(nameof(Subscription))]
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; }
}