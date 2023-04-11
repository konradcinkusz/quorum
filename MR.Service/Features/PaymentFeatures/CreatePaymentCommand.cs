namespace MR.Service.Features.PaymentFeatures;

public class CreatePaymentCommand : IRequest<int>
{
    public class CreatePaymentCommandHanlder : IRequestHandler<CreatePaymentCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreatePaymentCommandHanlder(IApplicationDbContext context)
        {
            _context = context;
        }

        public Task<int> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
