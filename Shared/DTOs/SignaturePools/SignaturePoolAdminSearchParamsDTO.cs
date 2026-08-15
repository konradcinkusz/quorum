namespace Quorum.Shared.DTOs.SignaturePools;

public class SignaturePoolAdminSearchParamsDTO : SearchParamsDTO
{
    public string? ApplicationUserId { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public int? Year { get; set; }
    public int? Quarter { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }

    public override void Clear()
    {
        this.ApplicationUserId = null; 
        this.ApplicationUserEmail = null;
        this.Year = null;
        this.Quarter = null;
        this.Begin = null;
        this.End = null;
    }
}
