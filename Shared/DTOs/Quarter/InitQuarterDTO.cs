namespace MR.Shared.DTOs.Quarter;

public class InitQuarterDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    //ile sygnatur należy dać wszystkich userom
    public int SignaturesCount { get; set; } = 3;
}
