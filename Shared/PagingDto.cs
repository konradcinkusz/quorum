namespace MR.Shared;

public abstract class PagedListDto<T> where T : class
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; } = 10;
    public List<T> Items { get; set; } = new List<T>();
}
