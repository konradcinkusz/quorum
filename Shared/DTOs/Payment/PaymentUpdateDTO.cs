namespace MR.Shared.DTOs.Payment;

public class PaymentUpdateDTO
{
    public PaymentUpdateDTO()
    {

    }
    public PaymentUpdateDTO(PaymentDTO paymentDTO)
    {
        Id = paymentDTO.Id;
        UserEmail = paymentDTO.UserEmail;
        PaymentLink = paymentDTO.PaymentLink;
        ClientReferenceId = paymentDTO.ClientReferenceId;
        PaymentIntentId = paymentDTO.PaymentIntentId;
        SessionId = paymentDTO.SessionId;
        PaymentValuePLN = paymentDTO.PaymentValuePLN;
    }
    public Guid Id { get; set; }
    public string UserEmail { get; set; }
    public string PaymentLink { get; set; }
    public string ClientReferenceId { get; set; }
    public string PaymentIntentId { get; set; }
    public string SessionId { get; set; }
    public decimal PaymentValuePLN { get; set; }
}
