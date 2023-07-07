namespace MR.Shared.DTOs.Quarter;

public class QuarterSearchParamsDTO : SearchParamsDTO
{
    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public int? Year { get; set; }
    public int? Quarter { get; set; }

    public override void Clear()
    {
        this.Year = null;
        this.Quarter = null;
        this.Begin = null;
        this.End = null;
    }
}
