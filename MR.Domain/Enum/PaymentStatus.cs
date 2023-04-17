namespace MR.Domain.Enums;

public enum PaymentStatus
{
    New,
    Pending,
    //stripe statuses
    CheckoutSessionAsyncPaymentFailed,
    CheckoutSessionAsyncPaymentSucceeded,
    CheckoutSessionCompleted,
    CheckoutSessionExpired,
    //
    Rejected,
    ActivatedByAdmin,
    DeactivatedByAdmin,
    AddedByCode,
    //zwykły przelew
    Completed,
    Unknown
}
