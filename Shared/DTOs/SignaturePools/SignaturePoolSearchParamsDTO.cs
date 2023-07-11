namespace MR.Shared.DTOs.SignaturePools;

public class SignaturePoolSearchParamsDTO : SearchParamsDTO
{
    public int? Year { get; set; }
    public int? Quarter { get; set; }

    public override void Clear()
    {
        this.Year = null;
        this.Quarter = null;
    }
}
