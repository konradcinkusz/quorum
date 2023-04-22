namespace MR.Shared.DTOs.Payment;

public class PaymentUpdateDTO
{
    public PaymentUpdateDTO()
    {

    }
    public PaymentUpdateDTO(PaymentDTO paymentDTO)
    {
        Id = paymentDTO.Id;
        ApplicationUserId = paymentDTO.ApplicationUserId;
        PaymentMethod = paymentDTO.PaymentMethod;
        ReferenceNumber = paymentDTO.ReferenceNumber;
        PaymentStatus = paymentDTO.PaymentStatus;
        PaymentValuePLN = paymentDTO.PaymentValuePLN;
    }
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; }
    public string PaymentMethod { get; set; }
    public string ReferenceNumber { get; set; }
    public decimal PaymentValuePLN { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }
}
