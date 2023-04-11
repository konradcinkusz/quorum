namespace MR.Domain.Entities;

[Table(nameof(TableNames.Payments), Schema = SchemasNames.MRPayments)]
public class Payment : BaseEntity<Guid>
{
    //null values
    public string? UserEmail { get; set; }
    public string? PaymentLink { get; set; }

    #region Stripe configuration
    /// <summary>
    /// A unique string to reference the Checkout Session. This can be a customer ID, a cart ID,
    /// or similar, and can be used to reconcile the Session with your internal systems.
    /// </summary>
    public string? ClientReferenceId { get; set; }
    /// <summary>
    /// (ID of the PaymentIntent)
    /// The ID of the PaymentIntent for Checkout Sessions in <c>payment</c> mode.
    /// paymentintentid:"pi_3Kl7iNDuPNEXFP7o0g1ZbcGC"
    /// </summary>
    public string? PaymentIntentId { get; set; }
    /// <summary>
    /// Unique identifier for the object. Used to pass to <c>redirectToCheckout</c> in
    /// Stripe.js.
    /// Id:"cs_test_a15y8jxRoyUni5iQLZxUV4W37ivkuBITSTnPuBQI220L4ZYgzLcvtaQ8iL"
    /// </summary>
    public string? SessionId { get; set; }
    /// <summary>
    /// The payment status of the Checkout Session, one of <c>paid</c>, <c>unpaid</c>, or
    /// <c>no_payment_required</c>. You can use this value to decide when to fulfill your
    /// customer's order.
    /// One of: <c>no_payment_required</c>, <c>paid</c>, or <c>unpaid</c>.
    /// </summary>
    public string? PaymentStatus { get; set; }
    #endregion

    [ForeignKey(nameof(ApplicationUser))]
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
    //Kwota płatności w PLN
    [Column(TypeName = "money")]
    public decimal PaymentValuePLN { get; set; }

    [InverseProperty(EntityNames.Payment)]
    public ICollection<PaymentStatusHistory> PaymentStatusHistories { get; set; }
}
