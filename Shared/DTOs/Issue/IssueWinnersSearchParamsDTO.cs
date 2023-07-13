namespace MR.Shared.DTOs.Issue;

public class IssueWinnersSearchParamsDTO : SearchParamsDTO
{
    public int? QuarterYear { get; set; }
    public int? QuarterNumber { get; set; }
}
