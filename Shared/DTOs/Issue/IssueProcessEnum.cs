namespace MR.Shared.DTOs.Issue;

public enum IssueProcessEnum
{
    Creation,
    Created,
    PaymentInitialized,
    PaymentInProgress,
    PaymentFailed,
    PaymentCanceled,
    PaymentCompleted,
    Publishing,
    InAdminVerification,
    AdminVerificationPassed,
    Published,
    EndedInCurrentQuarter
}