namespace MR.Domain.Enums;

public enum IssueProcess
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