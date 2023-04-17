namespace MR.Service;

public class Paging<T> where T : class
{
    public Paging()
    {
        CurrentPage = 1;
        PageSize = 100;
    }
    public Paging(int currentPage, int pageSize)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
    public Paging(QueryBase queryBase)
    {
        CurrentPage = queryBase.CurrentPage;
        PageSize = queryBase.PageSize;
    }
    public int CurrentPage { get; }
    public int PageSize { get; }
    public int TotalPages
    {
        get
        {
            return PageSize > 0 ? Items.Count() / PageSize : 0;
        }
    }
    public int TotalItems
    {
        get
        {
            return Items.Count();
        }
    }
    public List<T> Items { get; set; } = new List<T>();
}
public abstract class QueryBase
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Name { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
}
