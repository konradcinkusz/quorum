namespace Quorum.Shared.DTOs.Payment;

public enum PaymentStatusEnum
{
    None,
    New,
    Pending,
    Rejected,
    //sub to activated
    Accepted,
    Completed,
    Unknown,
    Refunded
}