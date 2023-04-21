namespace MR.Service.Features.Queries;

public class GetAdminLogsBySearchParamsQuery : QueryBase, IRequest<PagedList<AdminLog>>
{
    public class GetAdminLogsByQueryHandler : CommandHandlerBase<GetAdminLogsBySearchParamsQuery, PagedList<AdminLog>>
    {
        public GetAdminLogsByQueryHandler(IApplicationDbContext context, ILogger<GetAdminLogsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<AdminLog>> Handle(GetAdminLogsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Admin_Logs.AsQueryable();

            return new PagedList<AdminLog>(query, request);
        }
    }
}
