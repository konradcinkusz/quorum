namespace Quorum.Shared.DTOs.Issue.SearchParams;

public class IssueWinnersSearchParamsDTO : SearchParamsDTO
{
    public int? QuarterYear { get; set; }
    public int? QuarterNumber { get; set; }
}
