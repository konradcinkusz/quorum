namespace MR.Service;

public class PagedList<T> : List<T>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }

    public PagedList(IQueryable<T> query, QueryBase options)
    {
        CurrentPage = options.CurrentPage;
        PageSize = options.PageSize;

        TotalItems = query.Count();
        TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

        AddRange(query.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList());
    }
}
