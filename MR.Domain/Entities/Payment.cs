namespace MR.Domain.Entities;

[Table(nameof(TableNames.Payments), Schema = SchemasNames.MRPayments)]
public class Payment : BaseEntity<Guid>
{
    [ForeignKey(nameof(ApplicationUser))]
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
    //Kwota płatności w PLN
    [Column(TypeName = "money")]
    public decimal PaymentValuePLN { get; set; }
    public string PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    public string ReferenceNumber { get; set; } // a reference number associated with the payment (e.g. transaction ID)
    public PaymentStatus PaymentStatus { get; set; }
    [InverseProperty(EntityNames.Payment)]
    public ICollection<PaymentStatusHistory> PaymentStatusHistories { get; set; }
}

[Table(nameof(TableNames.Payment_Logs), Schema = SchemasNames.MRPayments)]
public class Payment_Log : BaseEntityLog
{
    [ForeignKey(nameof(Payment))]
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; }
}