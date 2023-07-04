using MR.Service.ViewModels;

namespace MR.Service.Features.PaymentFeatures;

public class SeedPaymentCommand : IRequest<PagedList<Payment>>
{
    public int Count { get; set; } = 1;
    public string ApplicationUserId { get; }

    public SeedPaymentCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }

    public class SeedPaymentCommandHandler : CommandHandlerBase<SeedPaymentCommand, PagedList<Payment>>
    {
        public SeedPaymentCommandHandler(IApplicationDbContext context, ILogger<SeedPaymentCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<PagedList<Payment>> Handle(SeedPaymentCommand request, CancellationToken cancellationToken)
        {
            if (request.Count < 1)
                request.Count = 1;

            var payments = new List<Payment>();

            for (int i = 0; i < request.Count; i++)
            {
                payments.Add(PaymentGenerator.GenerateRandomPayment(request.ApplicationUserId));
            }

            await _context.Payments.AddRangeAsync(payments, cancellationToken);

            await _context.Admin_Logs.AddAsync(new AdminLog
            {
                Action = "Seed Payments",
                Values = string.Join(", ", payments.Select(x => x.ReferenceNumber))
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Seeded {payments.Count} payments.");

            var query = _context.Payments.Where(x => payments.Select(x => x.Id).Contains(x.Id)).AsQueryable();

            return await PagedList<Payment>.CreateAsync(query, new SearchParams { PageSize = 1000 }, cancellationToken);
        }

    }
    public static class PaymentGenerator
    {
        private static readonly Random random = new Random();

        private static string GenerateReferenceNumber()
        {
            return $"pi_{random.Next(100000000, 999999999)}";
        }

        private static string GeneratePaymentMethod()
        {
            return $"sess_{random.Next(100000000, 999999999)}";
        }

        private static PaymentStatus GeneratePaymentStatus()
        {
            Array values = Enum.GetValues(typeof(PaymentStatus));
            return (PaymentStatus)values.GetValue(random.Next(values.Length));
        }

        public static Payment GenerateRandomPayment(string applicationUserId)
        {
            return new Payment
            {
                ReferenceNumber = GenerateReferenceNumber(),
                PaymentMethod = GeneratePaymentMethod(),
                PaymentStatus = GeneratePaymentStatus(),
                ApplicationUserId = applicationUserId,
                PaymentValuePLN = (decimal)random.Next(1000, 10000) / 100,
                PaymentStatusHistories = new List<PaymentStatusHistory> {
                new PaymentStatusHistory {
                    PaymentStatus = GeneratePaymentStatus(),
                },
                new PaymentStatusHistory {
                    PaymentStatus = GeneratePaymentStatus(),
                },
                new PaymentStatusHistory {
                    PaymentStatus = GeneratePaymentStatus(),
                }
            }
            };
        }
    }

}
