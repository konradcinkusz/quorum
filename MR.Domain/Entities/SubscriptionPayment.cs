namespace MR.Domain.Entities;

public class SubscriptionPayment
{
    [ForeignKey(nameof(Subscription)), Key, Column(Order = 0)]
    public string SubscriptionId { get; set; }
    public Subscription Subscription { get; set; }

    [ForeignKey(nameof(Payment)), Key, Column(Order = 1)]
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; }
}
