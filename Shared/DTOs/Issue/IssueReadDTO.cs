namespace MR.Shared.DTOs.Issue;

public class IssueReadDTO : BaseDTO
{
    public string ApplicationUserEmail { get; set; }
    public string ApplicationUserId { get; set; }
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    public IssueVisibilityEnum IssueVisibility { get; set; } = IssueVisibilityEnum.NotVisible;
    public IssueProcessEnum IssueProcess { get; set; } = IssueProcessEnum.InCreation;
    public int RatingValue { get; set; } = 0;
    public PaymentDTO? InitialPayment { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public List<IssueVisibilityHistoryDTO> IssueVisibilityHistories { get; set; } = new List<IssueVisibilityHistoryDTO>();
    public List<IssueProcessingHistoryDTO> IssueProcessingHistories { get; set; } = new List<IssueProcessingHistoryDTO>();
    public List<QuarterDTO> QuarterDTOs { get; set; } = new List<QuarterDTO>();
    public List<SignatureDTO> Signatures { get; set; } = new List<SignatureDTO>();
    public FileReadDTO PDF { get; set; }
}
