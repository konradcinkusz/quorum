namespace MR.Service.Features.QuarterFeatures.Queries;

public class GetQuartersBySearchParamsQuery : QueryBase, IRequest<PagedList<Quarter>>
{
    public int Year { get; set; }
    public int Month { get; set; }
    public class GetQuartersByQueryHandler : CommandHandlerBase<GetQuartersBySearchParamsQuery, PagedList<Quarter>>
    {
        public GetQuartersByQueryHandler(IApplicationDbContext context, ILogger<GetQuartersBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override Task<PagedList<Quarter>> Handle(GetQuartersBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
