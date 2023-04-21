namespace MR.Service.Features.PaymentFeatures.Queries;

public class GetPaymentQuery : IRequest<Payment>
{
    public Guid PaymentId { get; set; }

    public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Payment>
    {
        private readonly IApplicationDbContext _context;

        public GetPaymentQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentStatusHistories) // Include the PaymentStatusHistories related data
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null)
            {
                throw new NotFoundException(nameof(Payment), request.PaymentId);
            }

            return payment;
        }
    }
}


