namespace MR.Shared.DTOs.Payment;

public class PaymentSearchParamsDTO : SearchParamsDTO
{
    public Guid? PaymentId { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
    /// <summary>
    /// Only payment with relation to the issue initial payment
    /// </summary>
    public bool? OnlyInitialPayment { get; set; } = null;
}

