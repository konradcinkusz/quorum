namespace MR.Shared.DTOs.Issue;

public class IssueSearchParamsDTO : SearchParamsDTO
{
    public Guid? IssueId { get; set; }
    public string? CreatedByEmail { get; set; }
    public string? Title { get; set; }
    public string? Question { get; set; }
    public bool? IsVerifyByAdmin { get; set; }
    public IssueStatusEnum? IssueStatus { get; set; }
    public int? RatingValue { get; set; }
    public bool? HasInitialPayment { get; set; }
}
