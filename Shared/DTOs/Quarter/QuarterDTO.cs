namespace MR.Shared.DTOs.Quarter;

public class QuarterDTO
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<IssueReadDTO> Issues { get; set; } = new List<IssueReadDTO>();
    public List<SignaturePoolDTO> SignaturePools { get; set; } = new List<SignaturePoolDTO>();
}