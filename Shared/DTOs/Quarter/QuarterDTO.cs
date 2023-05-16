using MR.Shared.DTOs.Issue;
using MR.Shared.DTOs.SignaturePools;

namespace MR.Shared.DTOs.Quarter;

public class QuarterDTO
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<IssueDTO> Issues { get; set; } = new List<IssueDTO>();
    public List<SignaturePoolDTO> SignaturePools { get; set; } = new List<SignaturePoolDTO>();
}