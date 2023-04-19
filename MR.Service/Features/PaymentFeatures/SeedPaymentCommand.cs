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
                        UserEmail = "user1@example.com",
                        PaymentLink = "https://example.com/payments/12345",
                        ClientReferenceId = "ref123",
                        PaymentIntentId = "pi_123456789",
                        SessionId = "sess_123456789",
                        PaymentStatus = PaymentStatus.Unknown.ToString(),
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
                        UserEmail = "user2@example.com",
                        PaymentLink = "https://example.com/payments/67890",
                        ClientReferenceId = "ref456",
                        PaymentIntentId = "pi_987654321",
                        SessionId = "sess_987654321",
                        PaymentStatus = PaymentStatus.Rejected.ToString(),
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
                        UserEmail = "user3@example.com",
                        PaymentLink = "https://example.com/payments/24680",
                        ClientReferenceId = "ref789",
                        PaymentIntentId = "pi_135792468",
                        SessionId = "sess_135792468",
                        PaymentStatus = PaymentStatus.Pending.ToString(),
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
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Seeded {payments.Count} payments.");

            return Unit.Value;
        }

    }
}
