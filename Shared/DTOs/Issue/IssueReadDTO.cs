namespace MR.Shared.DTOs.Issue;

public class IssueReadDTO : BaseDTO
{
    public string ApplicationUserEmail { get; set; }
    public string ApplicationUserId { get; set; }
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    public IssueStatusEnum IssueStatus { get; set; } = IssueStatusEnum.NotVisible;
    public int RatingValue { get; set; } = 0;
    public PaymentDTO? InitialPayment { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public List<IssueStatusHistoryDTO> IssueStatusHistories { get; set; }
}
