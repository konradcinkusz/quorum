using Quorum.Service.ViewModels;

namespace Quorum.Service.Features.PaymentFeatures.Queries;

public class GetPaymentsBySearchParamsQuery : QueryBase, IRequest<PagedList<Payment>>
{
    public Guid? PaymentId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public decimal? PaymentValuePLN { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
    public bool? OnlyInitialPayment { get; set; }

    public class GetPaymentsByQueryHandler : CommandQueryHandlerBase<GetPaymentsBySearchParamsQuery, PagedList<Payment>>
    {
        public GetPaymentsByQueryHandler(IApplicationDbContext context, ILogger<GetPaymentsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Payment>> Handle(GetPaymentsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Payment> query = _context.Payments
                .Include(x => x.RelatedIssue)
                .Include(x => x.PaymentStatusHistories)
                .AsQueryable();

            query = ApplyUserFilter(query, request);

            if (request.OnlyInitialPayment.HasValue)
            {
                if (request.OnlyInitialPayment.Value)
                {
                    query = query.Where(x => x.RelatedIssue != null);
                }
                else
                {
                    query = query.Where(x => x.RelatedIssue == null);
                }
            }

            if (request.PaymentId.HasValue && request.PaymentId.Value != Guid.Empty)
            {
                query = query.Where(p => p.Id == request.PaymentId.Value);
            }

            if (!string.IsNullOrEmpty(request.PaymentMethod))
            {
                query = query.Where(p => p.PaymentMethod == request.PaymentMethod);
            }

            if (!string.IsNullOrEmpty(request.ReferenceNumber))
            {
                query = query.Where(p => p.ReferenceNumber == request.ReferenceNumber);
            }

            if (request.PaymentStatus.HasValue && request.PaymentStatus != Domain.Enums.PaymentStatus.None)
            {
                query = query.Where(p => p.PaymentStatus == request.PaymentStatus);
            }

            if (request.MinPaymentValuePLN.HasValue)
            {
                query = query.Where(p => p.PaymentValuePLN >= request.MinPaymentValuePLN.Value);
            }

            if (request.MaxPaymentValuePLN.HasValue)
            {
                query = query.Where(p => p.PaymentValuePLN <= request.MaxPaymentValuePLN.Value);
            }

            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            var pagedList = await PagedList<Payment>.CreateAsync(query, request.SearchParams, cancellationToken);

            await _context.PopulateUserEmailsAsync(
                pagedList, x => x.ApplicationUserId, (x, email) => x.ApplicationUserEmail = email, cancellationToken);

            return pagedList;
        }
    }
}
