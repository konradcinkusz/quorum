namespace Quorum.Shared.DTOs.Quarter;

public class QuarterDTO : BaseDTO
{
    public int Year { get; set; }
    public int QuarterNumber { get; set; }
    public List<IssueReadDTO> Issues { get; set; } = new List<IssueReadDTO>();
    public List<SignaturePoolAdminDTO> SignaturePools { get; set; } = new List<SignaturePoolAdminDTO>();
    public int PrimarySignatureCount { get; set; }
    public bool QuarterResolved { get; set; }
    public IssueReadDTO? QuarterWinner { get; set; }
}