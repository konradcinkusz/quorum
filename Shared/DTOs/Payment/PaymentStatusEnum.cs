namespace MR.Shared.DTOs.Payment;

public enum PaymentStatusEnum
{
    New,
    Pending,
    Rejected,
    ActivatedByAdmin,
    DeactivatedByAdmin,
    AddedByCode,
    Completed,
    Unknown,
    Refunded
}