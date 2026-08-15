namespace Quorum.Service.Features.PaymentFeatures;

internal abstract class PaymentCommandHandlerBase<TCommand, TResult> : CommandHandlerBase<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    public PaymentCommandHandlerBase(IApplicationDbContext context, ILogger<TCommand> logger) : base(context, logger)
    {
    }

    protected void SetPaymentStatus(Payment payment, PaymentStatus paymentStatus)
    {
        payment.PaymentStatusHistories = new List<PaymentStatusHistory> { new PaymentStatusHistory { PaymentStatus = paymentStatus } };
        payment.PaymentStatus = paymentStatus;
    }
}