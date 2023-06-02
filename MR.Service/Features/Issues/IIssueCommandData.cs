namespace MR.Service.Features.Issues;

public interface IIssueCommandData
{
    string CreatedById { get; }
    Guid IssueId { get; }
}
