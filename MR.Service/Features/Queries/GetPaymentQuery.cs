namespace MR.Service.Features.Queries;

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
            var payment = await _context.Payments.FindAsync(request.PaymentId);

            if (payment == null)
            {
                throw new NotFoundException(nameof(Payment), request.PaymentId);
            }

            return payment;
        }
    }
}


