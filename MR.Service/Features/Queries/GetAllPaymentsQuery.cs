namespace MR.Service.Features.Queries;

public class GetAllPaymentsQuery : IRequest<IEnumerable<Payment>>
{
    public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, IEnumerable<Payment>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllPaymentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Payments.ToListAsync(cancellationToken);
        }
    }
}
