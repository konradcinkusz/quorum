namespace MR.Service.Features.Queries;

public class GetAdminLogsBySearchParamsQuery : QueryBase, IRequest<PagedList<AdminLog>>
{
    public string? Action { get; set; }
    public string? ValuesText { get; set; }
    public bool? LastMonth { get; set; }
    public bool? LastHour { get; set; }

    public class GetAdminLogsByQueryHandler : CommandHandlerBase<GetAdminLogsBySearchParamsQuery, PagedList<AdminLog>>
    {
        public GetAdminLogsByQueryHandler(IApplicationDbContext context, ILogger<GetAdminLogsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<AdminLog>> Handle(GetAdminLogsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Admin_Logs.AsQueryable();

            // apply context search
            if (!string.IsNullOrEmpty(request.ValuesText))
            {
                query = query.Where(log => log.Values != null && log.Values.Contains(request.ValuesText));
            }

            if (!string.IsNullOrEmpty(request.Action))
            {
                query = query.Where(log => log.Action != null && log.Action.Contains(request.Action));
            }

            if (request.LastMonth.HasValue && request.LastMonth.Value)
            {
                var lastMonth = DateTime.UtcNow.AddDays(-30);
                query = query.Where(log => log.CreatedAt >= lastMonth);
            }

            if (request.LastHour.HasValue && request.LastHour.Value)
            {
                var lastHour = DateTime.UtcNow.AddHours(-1);
                query = query.Where(log => log.CreatedAt >= lastHour);
            }

            return await PagedList<AdminLog>.CreateAsync(query, request.SearchParams, cancellationToken);
        }
    }
}
