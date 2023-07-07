namespace MR.Shared.DTOs.Issue;

public class IssueSearchParamsDTO : SearchParamsDTO
{
    public Guid? IssueId { get; set; }
    public string? CreatedByEmail { get; set; }
    public string? Title { get; set; }
    public string? Question { get; set; }
    public bool? IsVerifyByAdmin { get; set; }
    public IssueVisibilityEnum? IssueVisibility { get; set; }
    public int? RatingValue { get; set; }
    public int? QuarterYear { get; set; }
    public int? QuarterNumber { get; set; }
    public IssuePaymentOptions? PaymentOptions { get; set; }
    public bool Archived { get; set; } = false;
}
