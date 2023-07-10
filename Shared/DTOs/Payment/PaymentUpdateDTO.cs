namespace MR.Shared.DTOs.Payment;

public class PaymentUpdateDTO
{
    public PaymentUpdateDTO()
    {

    }

    public PaymentUpdateDTO(PaymentDTO paymentDTO)
    {
        ApplicationUserId = paymentDTO.ApplicationUserId;
        PaymentMethod = paymentDTO.PaymentMethod;
        ReferenceNumber = paymentDTO.ReferenceNumber;
        PaymentStatus = paymentDTO.PaymentStatus;
        PaymentValuePLN = paymentDTO.PaymentValuePLN;
    }

    public string ApplicationUserId { get; set; }
    public string PaymentMethod { get; set; }
    public string ReferenceNumber { get; set; }
    public decimal PaymentValuePLN { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }
}
