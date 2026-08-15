namespace Quorum.Domain.Entities;

[Table(nameof(TableNames.Payments), Schema = SchemasNames.MRPayments)]
public class Payment : BaseEntity<Guid>
{
    /// <summary>Subject id from the identity service; no navigation on purpose (ADR 0001).</summary>
    public string ApplicationUserId { get; set; }

    /// <summary>Display email resolved from the <see cref="QuorumUser"/> projection by the
    /// query layer; not a column, and never authoritative.</summary>
    [NotMapped]
    public string? ApplicationUserEmail { get; set; }
    //Kwota płatności w PLN
    [Column(TypeName = "money")]
    public decimal PaymentValuePLN { get; set; }
    public string? PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    public string? ReferenceNumber { get; set; } // a reference number associated with the payment (e.g. transaction ID)
    public PaymentStatus PaymentStatus { get; set; }
    [InverseProperty(EntityNames.Payment)]
    public ICollection<PaymentStatusHistory> PaymentStatusHistories { get; set; }
    [InverseProperty(EntityNames.Payment)]
    public ICollection<SubscriptionPayment> SubscriptionPayments { get; set; }
    [InverseProperty(nameof(Payment_Log.Payment))]
    public ICollection<Payment_Log> Payment_Logs { get; set; }

    [InverseProperty(nameof(Issue.InitialPayment))]
    public Issue? RelatedIssue { get; set; }
}


[Table(nameof(TableNames.Payment_Logs), Schema = SchemasNames.MRPayments)]
public class Payment_Log : BaseEntityLog
{
    [ForeignKey(nameof(Payment))]
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; }
}