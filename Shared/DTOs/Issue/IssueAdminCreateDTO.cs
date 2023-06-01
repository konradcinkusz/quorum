namespace MR.Shared.DTOs.Issue;

public class IssueAdminCreateDTO : IssueCreateDTO
{
    public string? ApplicationUserEmail { get; set; }
    //nullable
    public string ApplicationUserId { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    public int RatingValue { get; set; } = 0;
    public IssueStatusEnum IssueStatus { get; set; } = IssueStatusEnum.NotVisible;
    public PaymentDTO? InitialPayment { get; set; }
}
