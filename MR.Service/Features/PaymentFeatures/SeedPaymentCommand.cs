namespace MR.Service.Features.PaymentFeatures;

public class SeedPaymentCommand : IRequest
{
    public string ApplicationUserId { get; }

    public SeedPaymentCommand(string applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }

    public class SeedPaymentCommandHandler : CommandHandlerBase<SeedPaymentCommand, Unit>
    {

        public SeedPaymentCommandHandler(IApplicationDbContext context, ILogger<SeedPaymentCommand> logger)
            : base(context, logger)
        {
        }

        public override async Task<Unit> Handle(SeedPaymentCommand request, CancellationToken cancellationToken)
        {
            var payments = new List<Payment>
                {
                    new Payment
                    {
                        ReferenceNumber = "pi_123456789",
                        PaymentMethod = "sess_123456789",
                        PaymentStatus = PaymentStatus.Unknown,
                        ApplicationUserId = request.ApplicationUserId,
                        PaymentValuePLN = 100.00M,
                        PaymentStatusHistories = new List<PaymentStatusHistory> {
                            new PaymentStatusHistory {
                                PaymentStatus = PaymentStatus.New,
                            },
                            new PaymentStatusHistory {
                                PaymentStatus = PaymentStatus.Pending,
                            },
                            new PaymentStatusHistory {
                                PaymentStatus = PaymentStatus.Unknown,
                            }
                        }
                    },
                    new Payment
                    {
                        ReferenceNumber = "pi_123456789",
                        PaymentMethod = "sess_123456789",
                        PaymentStatus = PaymentStatus.Unknown,
                        ApplicationUserId = request.ApplicationUserId,
                        PaymentValuePLN = 50.00M,
                        PaymentStatusHistories = new List<PaymentStatusHistory> {
                            new PaymentStatusHistory {
                                PaymentStatus = PaymentStatus.New
                            },
                            new PaymentStatusHistory {
                                PaymentStatus = PaymentStatus.Rejected
                            }
                        }
                    },
                    new Payment
                    {
                        ReferenceNumber = "pi_123456789", PaymentMethod = "sess_123456789", PaymentStatus = PaymentStatus.Unknown,
                        ApplicationUserId = request.ApplicationUserId,
                        PaymentValuePLN = 200.00M,
                        PaymentStatusHistories = new List<PaymentStatusHistory> {
                            new PaymentStatusHistory {
                                PaymentStatus = PaymentStatus.New
                            },
                            new PaymentStatusHistory {
                                PaymentStatus = PaymentStatus.Pending
                            }
                        }
                    }
                };

            

            await _context.Payments.AddRangeAsync(payments, cancellationToken);

            await _context.Admin_Logs.AddAsync(new AdminLog
            {
                Action = "Seed Payments",
                Values = string.Join(", ", new List<string> { "pi_123456789", "pi_987654321", "pi_135792468" })
            }); ;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Seeded {payments.Count} payments.");

            return Unit.Value;
        }

    }
}
