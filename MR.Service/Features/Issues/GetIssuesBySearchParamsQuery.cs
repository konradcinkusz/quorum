using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MR.Service.Features.Issues;

public class GetIssuesBySearchParamsQuery : QueryBase, IRequest<PagedList<Issue>>
{
    public Guid? IssueId { get; set; }
    public string? CreatedById { get; set; }
    public string? CreatedByEmail { get; set; }
    public string? Title { get; set; }
    public string? Question { get; set; }
    public bool? IsVerifyByAdmin { get; set; }
    public IssueVisibility? IssueVisibility { get; set; }
    public int? RatingValue { get; set; }
    public bool? HasInitialPayment { get; set; }
    public int? QuarterYear { get; set; }
    public int? QuarterNumber { get; set; }
    public class GetIssuesBySearchParamsQueryHandler : CommandQueryHandlerBase<GetIssuesBySearchParamsQuery, PagedList<Issue>>
    {
        public GetIssuesBySearchParamsQueryHandler(IApplicationDbContext context, ILogger<GetIssuesBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Issue>> Handle(GetIssuesBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Issues
                .Include(x => x.InitialPayment)
                .Include(x => x.QuarterIssues).ThenInclude(qi => qi.Quarter)
                .Include(x => x.IssueVisibilityHistories)
                .Include(x => x.IssueProcessingHistories)
                .Include(x => x.CreatedBy).AsQueryable();

            if (request.IssueId.HasValue)
            {
                query = query.Where(x => x.Id == request.IssueId.Value);
            }

            if (!string.IsNullOrEmpty(request.CreatedByEmail))
            {
                query = query.Where(x => x.CreatedBy != null && !string.IsNullOrEmpty(x.CreatedBy.Email) &&
                            x.CreatedBy.Email.Contains(request.CreatedByEmail));
            }

            if (!string.IsNullOrEmpty(request.CreatedById))
            {
                query = query.Where(x => !string.IsNullOrEmpty(x.CreatedById) &&
                    x.CreatedById == request.CreatedById);
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                query = query.Where(x => x.Title != null && x.Title.Contains(request.Title));
            }

            if (!string.IsNullOrEmpty(request.Question))
            {
                query = query.Where(x => x.Question != null && x.Question.Contains(request.Question));
            }

            if (request.IsVerifyByAdmin.HasValue)
            {
                query = query.Where(x => x.IsVerifyByAdmin == request.IsVerifyByAdmin.Value);
            }

            if (request.IssueVisibility.HasValue)
            {
                query = query.Where(x => x.IssueVisibility == request.IssueVisibility.Value);
            }

            if (request.RatingValue.HasValue)
            {
                query = query.Where(x => x.RatingValue == request.RatingValue.Value);
            }

            if (request.HasInitialPayment.HasValue)
            {
                query = query.Where(x => request.HasInitialPayment.Value ? x.InitialPayment != null : x.InitialPayment == null);
            }

            if (request.QuarterYear.HasValue)
            {
                query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.Year == request.QuarterYear.Value));
            }

            if (request.QuarterNumber.HasValue)
            {
                query = query.Where(p => p.QuarterIssues.Any(y => y.Quarter.QuarterNumber == request.QuarterNumber.Value));
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<Issue>.CreateAsync(query, request.SearchParams, cancellationToken);

            return pagedList;
        }

        protected override IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortColumn, SortOrder sortOrder)
        {
            if (!string.IsNullOrEmpty(sortColumn) && sortOrder != SortOrder.Unspecified)
            {
                switch (sortColumn)
                {
                    case "ApplicationUserEmail":
                        if (sortOrder == SortOrder.Ascending)
                        {
                            query = query.OrderBy(p => (p as Issue).CreatedBy.Email);
                        }
                        else if (sortOrder == SortOrder.Descending)
                        {
                            query = query.OrderByDescending(p => (p as Issue).CreatedBy.Email);
                        }
                        break;
                    default:
                        query = base.ApplySorting(query, sortColumn, sortOrder);
                        break;
                }
            }
            return query;
        }
    }
}
