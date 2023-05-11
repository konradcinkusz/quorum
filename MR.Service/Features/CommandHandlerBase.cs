namespace MR.Service.Features;

public abstract class CommandHandlerBase<TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    protected readonly IApplicationDbContext _context;
    protected readonly ILogger<TCommand> _logger;

    protected CommandHandlerBase(IApplicationDbContext context, ILogger<TCommand> logger)
    {
        _context = context;
        _logger = logger;
    }

    public abstract Task<TResult> Handle(TCommand request, CancellationToken cancellationToken);

    protected IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortColumn, SortOrder sortOrder)
    {
        if (!string.IsNullOrEmpty(sortColumn))
        {
            if (sortOrder == SortOrder.Ascending)
            {
                query = query.OrderBy(sortColumn);
            }
            else
            {
                query = query.OrderByDescending(sortColumn);
            }
        }

        return query;
    }
}
