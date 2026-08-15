namespace Quorum.Shared.DTOs.Issue.SignAndSubmit;

public class IssueSignedAndSubmittedDTO : PublicPublishedEndedIssueRead
{
    public IssueSignedAndSubmittedProcessEnum IssueSignedAndSubmittedProcess { get; set; }
}
