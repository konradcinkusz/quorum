namespace Quorum.Domain.Entities;

[Table(nameof(TableNames.PaymentStatusHistories), Schema = SchemasNames.MRPayments)]
public class PaymentStatusHistory : BaseEntity<Guid>
{
    public PaymentStatus PaymentStatus { get; set; }
    [ForeignKey(nameof(Payment))]
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; }
}
