namespace MR.Shared.DTOs.SignaturePools;

public class SignaturePoolsSearchParamsDTO : SearchParamsDTO
{
    public string? ApplicationUserId { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public int? Year { get; set; }
    public int? Quarter { get; set; }
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
}
