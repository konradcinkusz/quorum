namespace MR.Service.Features.PaymentFeatures.Queries;

public class GetPaymentsBySearchParamsQuery : QueryBase, IPaymentBaseFeature, IRequest<PagedList<Payment>>
{
    public PaymentStatus PaymentStatus { get; set; }
    public string ApplicationUserId { get; set; }
    public decimal PaymentValuePLN { get; set; }
    public string PaymentMethod { get; set; } // the payment method used (e.g. credit card, PayPal, etc.)
    public string ReferenceNumber { get; set; }// a reference number associated with the payment (e.g. transaction ID)
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }
    public string SortColumn { get; set; }
    public SortOrder SortOrder { get; set; }

    public class GetPaymentsByQueryHandler : CommandHandlerBase<GetPaymentsBySearchParamsQuery, PagedList<Payment>>
    {
        public GetPaymentsByQueryHandler(IApplicationDbContext context, ILogger<GetPaymentsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<PagedList<Payment>> Handle(GetPaymentsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Payments.Include(x => x.PaymentStatusHistories).Include(x => x.ApplicationUser).AsQueryable();

            if (!string.IsNullOrEmpty(request.ApplicationUserId))
            {
                query = query.Where(p => p.ApplicationUserId.Contains(request.ApplicationUserId));
            }

            if (!string.IsNullOrEmpty(request.PaymentMethod))
            {
                query = query.Where(p => p.PaymentMethod == request.PaymentMethod);
            }

            if (!string.IsNullOrEmpty(request.ReferenceNumber))
            {
                query = query.Where(p => p.ReferenceNumber == request.ReferenceNumber);
            }

            if (request.PaymentStatus != PaymentStatus.None)
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

            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                if (request.SortOrder == SortOrder.Ascending)
                {
                    query = query.OrderBy(request.SortColumn);
                }
                else
                {
                    query = query.OrderByDescending(request.SortColumn);
                }
            }
            return new PagedList<Payment>(query, request.SearchParams);
        }
    }
}
