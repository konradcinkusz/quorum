namespace MR.Shared;

public abstract class SearchParams
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
}
