namespace Quorum.Shared.DTOs.Issue.Admin;

public sealed class IssueAdminCreateDTO : IssueCreateDTO
{
    public string? ApplicationUserEmail { get; set; }
    //nullable
    public string ApplicationUserId { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    public int RatingValue { get; set; } = 0;
    public IssueVisibilityEnum IssueVisibility { get; set; }
    public IssueProcessEnum IssueProcess { get; set; }
    public PaymentDTO? InitialPayment { get; set; }
}
