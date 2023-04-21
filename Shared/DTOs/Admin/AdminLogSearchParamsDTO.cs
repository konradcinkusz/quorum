namespace MR.Shared.DTOs.Payment;

public class AdminLogSearchParamsDTO : SearchParams
{
    public string Action { get; set; } = string.Empty;
    public string ValuesText { get; set; } = string.Empty;
    public bool LastMonth { get; set; } = false;
    public bool LastHour { get; set; } = false;
}
