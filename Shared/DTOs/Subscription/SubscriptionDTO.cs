namespace Quorum.Shared.DTOs.Subscription;

public class SubscriptionDTO
{
    //PK
    public string ApplicationUserId { get; set; }
    public string ApplicationUserEmail { get; set; }

    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public bool IsActive { get; set; }

    public List<PaymentDTO> PaymentDTOs { get; set; }
}
