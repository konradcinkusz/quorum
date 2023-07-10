namespace MR.Shared.DTOs.Quarter;

public class QuarterDTO : BaseDTO
{
    public int Year { get; set; }
    public int QuarterNumber { get; set; }
    public List<IssueReadDTO> Issues { get; set; } = new List<IssueReadDTO>();
    public List<SignaturePoolDTO> SignaturePools { get; set; } = new List<SignaturePoolDTO>();
    public int PrimarySignatureCount { get; set; }
}