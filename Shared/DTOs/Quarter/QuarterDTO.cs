using MR.Shared.DTOs.Issue;

namespace MR.Shared.DTOs.Quarter;

public class QuarterDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<IssueDTO> Issues { get; set; }
}