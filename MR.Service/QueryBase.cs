namespace MR.Service;

public abstract class QueryBase
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Name { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
}
