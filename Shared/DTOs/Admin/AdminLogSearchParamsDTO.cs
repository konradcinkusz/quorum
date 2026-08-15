namespace Quorum.Shared.DTOs.Payment;

public class AdminLogSearchParamsDTO : SearchParamsDTO
{
    public string? Action { get; set; }
    public string? ValuesText { get; set; }
    public bool? LastMonth { get; set; }
    public bool? LastHour { get; set; }

    public override void Clear()
    {
        this.Action = null;
        this.ValuesText = null;
        this.LastMonth = null;
        this.LastHour = null;
    }
}
