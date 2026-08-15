namespace Quorum.Shared.DTOs.Payment;

public class PaymentCreateDTO
{
    public decimal PaymentValuePLN { get; set; }
    public string PaymentMethod { get; set; } 
    public string ReferenceNumber { get; set; }
}
