namespace Quorum.Service.ViewModels;

public class PagedList<T> : List<T>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }

    private PagedList() { } // Private constructor to prevent direct instantiation

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, SearchParams options, CancellationToken cancellationToken = default)
    {
        var pagedList = new PagedList<T>
        {
            CurrentPage = options.CurrentPage,
            PageSize = options.PageSize
        };

        pagedList.TotalItems = await query.CountAsync(cancellationToken);
        pagedList.TotalPages = (int)Math.Ceiling(pagedList.TotalItems / (double)pagedList.PageSize);

        // Check if the new PageSize would result in a page number greater than the total number of pages
        if (pagedList.CurrentPage > pagedList.TotalPages && pagedList.TotalPages > 0)
        {
            pagedList.CurrentPage = pagedList.TotalPages;
        }

        var items = await query.Skip((pagedList.CurrentPage - 1) * pagedList.PageSize).Take(pagedList.PageSize).ToListAsync(cancellationToken);
        pagedList.AddRange(items);

        return pagedList;
    }

    public static PagedList<T> Create(IEnumerable<T> source, SearchParams options)
    {
        var pagedList = new PagedList<T>
        {
            CurrentPage = options.CurrentPage,
            PageSize = options.PageSize
        };

        pagedList.TotalItems = source.Count();
        pagedList.TotalPages = (int)Math.Ceiling(pagedList.TotalItems / (double)pagedList.PageSize);

        // Check if the new PageSize would result in a page number greater than the total number of pages
        if (pagedList.CurrentPage > pagedList.TotalPages && pagedList.TotalPages > 0)
        {
            pagedList.CurrentPage = pagedList.TotalPages;
        }

        var items = source.Skip((pagedList.CurrentPage - 1) * pagedList.PageSize).Take(pagedList.PageSize);
        pagedList.AddRange(items);

        return pagedList;
    }
}

