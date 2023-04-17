namespace MR.Service.Features.Queries;

public class GetPaymentsBySearchParamsQuery : QueryBase, IRequest<Paging<Payment>>
{
    public string UserEmail { get; set; } = string.Empty;
    public string ClientReferenceId { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public decimal? MinPaymentValuePLN { get; set; }
    public decimal? MaxPaymentValuePLN { get; set; }

    public class GetPaymentsByQueryHandler : CommandHandlerBase<GetPaymentsBySearchParamsQuery, Paging<Payment>>
    {
        public GetPaymentsByQueryHandler(IApplicationDbContext context, ILogger<GetPaymentsBySearchParamsQuery> logger) : base(context, logger)
        {
        }

        public override async Task<Paging<Payment>> Handle(GetPaymentsBySearchParamsQuery request, CancellationToken cancellationToken)
        {
                var query = _context.Payments.AsQueryable();

                if (!string.IsNullOrEmpty(request.UserEmail))
                {
                    query = query.Where(p => p.UserEmail.Contains(request.UserEmail));
                }

                if (!string.IsNullOrEmpty(request.ClientReferenceId))
                {
                    query = query.Where(p => p.ClientReferenceId == request.ClientReferenceId);
                }

                if (!string.IsNullOrEmpty(request.PaymentIntentId))
                {
                    query = query.Where(p => p.PaymentIntentId == request.PaymentIntentId);
                }

                if (request.PaymentStatus != PaymentStatus.Unknown)
                {
                    query = query.Where(p => p.PaymentStatus == request.PaymentStatus.ToString());
                }

                if (request.MinPaymentValuePLN.HasValue)
                {
                    query = query.Where(p => p.PaymentValuePLN >= request.MinPaymentValuePLN.Value);
                }

                if (request.MaxPaymentValuePLN.HasValue)
                {
                    query = query.Where(p => p.PaymentValuePLN <= request.MaxPaymentValuePLN.Value);
                }

                // Count total number of items
                int totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                int skip = (request.CurrentPage - 1) * request.PageSize;
                query = query.Skip(skip).Take(request.PageSize);

                var payments = await query.ToListAsync(cancellationToken);

                return new Paging<Payment>(request)
                {
                    Items = payments
                };
            
        }
    }
}
