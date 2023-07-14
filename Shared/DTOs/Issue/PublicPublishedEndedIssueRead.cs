namespace MR.Shared.DTOs.Issue;

/// <summary>
/// After resolving the quarter we have to define prop indicates if the user win the quarter
/// </summary>
public class PublicPublishedEndedIssueRead : PublicPublishedIssueRead
{
    public bool Winner { get; set; }
    public int? QuarterYear { get; set; }
    public int? QuarterNumber { get; set; }
}
