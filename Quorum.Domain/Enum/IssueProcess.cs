namespace Quorum.Domain.Enums;

public enum IssueProcess
{
    //Create
    InCreation, // --> go to created
    Created, // --> go to paymentInitialized
    //Pay for an issue
    PaymentInitialized, // --> go to payment in progress
    PaymentInProgress, // --> go to failed, canceled or completed
    PaymentFailed, // --> go to created
    PaymentCanceled, // --> go to created
    PaymentCompleted, // --> go to InAdminVerification
    //Verify by Admin
    InAdminVerification, // --> go to passed or failed
    AdminVerificationPassed, // --> go to publishing
    AdminVerificationFailed, // --> go to created
    //Publish
    Publishing, // --> go to published
    PublishingAgain,
    Published, // --> go to EndedInCurrentQuarter
    //End
    EndedInCurrentQuarter, // --> go to PublishingAgain, deleted or finished
    //Delete
    Deleted,
    //Finish
    Finished
}