namespace MR.Service.ViewModels;

public class SearchParams
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = int.MaxValue;
}
