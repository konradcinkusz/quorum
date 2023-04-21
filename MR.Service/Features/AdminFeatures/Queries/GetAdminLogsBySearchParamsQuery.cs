namespace MR.Service.Features.Queries;

public class GetAdminLogsBySearchParamsQuery : QueryBase, IRequest<PagedList<AdminLog>>
{
    public string Action { get; set; } = string.Empty;
    public string ValuesText { get; set; } = string.Empty;
    public bool LastMonth { get; set; } = false;
    public bool LastHour { get; set; } = false;

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
                query = query.Where(log => log.Values.Contains(request.ValuesText));
            }

            if (!string.IsNullOrEmpty(request.Action))
            {
                query = query.Where(log => log.Action.Contains(request.ValuesText));
            }

            if (request.LastMonth)
            {
                var lastMonth = DateTime.UtcNow.AddDays(-30);
                query = query.Where(log => log.CreatedAt >= lastMonth);
            }

            if (request.LastHour)
            {
                var lastHour = DateTime.UtcNow.AddHours(-1);
                query = query.Where(log => log.CreatedAt >= lastHour);
            }

            return new PagedList<AdminLog>(query, request);
        }
    }
}
