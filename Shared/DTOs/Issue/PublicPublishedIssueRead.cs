namespace MR.Shared.DTOs.Issue;

public class PublicPublishedIssueRead : BaseDTO
{
    public string ApplicationUserEmail { get; set; }
    public string Title { get; set; }
    public string Question { get; set; }
    public int RatingValue { get; set; } = 0;
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
}