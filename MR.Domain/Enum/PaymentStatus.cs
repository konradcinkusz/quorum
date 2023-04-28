namespace MR.Domain.Enums;

public enum PaymentStatus
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