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
