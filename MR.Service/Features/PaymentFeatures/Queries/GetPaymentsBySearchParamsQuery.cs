namespace MR.Service.Features.PaymentFeatures.Queries;

public class GetPaymentsBySearchParamsQuery : QueryBase, IRequest<PagedList<Payment>>
{
    public Guid? PaymentId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public decimal? PaymentValuePLN { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }

    public class GetPaymentsByQueryHandler : CommandHandlerBase<GetPaymentsBySearchParamsQuery, PagedList<Payment>>
    {
        public GetPaymentsByQueryHandler(IApplicationDbContext context, ILogger<GetPaymentsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Payment>> Handle(GetPaymentsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Payments.Include(x => x.PaymentStatusHistories).Include(x => x.ApplicationUser).AsQueryable();

            if (!string.IsNullOrEmpty(request.ApplicationUserEmail))
            {
                query = query.Where(p => p.ApplicationUser != null && !string.IsNullOrEmpty(p.ApplicationUser.Email) && p.ApplicationUser.Email.Contains(request.ApplicationUserEmail));
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

            return await PagedList<Payment>.CreateAsync(query, request.SearchParams, cancellationToken);
        }
    }
}
