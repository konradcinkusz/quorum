namespace Quorum.Shared.DTOs.Issue;

public enum IssueProcessEnum
{
    InCreation,
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