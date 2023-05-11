namespace MR.Service;

public abstract class QueryBase
{
    public SearchParams SearchParams { get; set; } = new SearchParams();
    public string SortColumn { get; set; }
    public SortOrder SortOrder { get; set; }
}
