namespace MR.Shared.DTOs.Payment;

public class PaymentDTO
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; }
    public string PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    public string ReferenceNumber { get; set; } // a reference number associated with the payment (e.g. transaction ID)

    public DateTime CreatedAt { get; set; }
    public decimal PaymentValuePLN { get; set; }
    public List<PaymentStatusHistoryDTO> PaymentStatusHistories { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }
}
