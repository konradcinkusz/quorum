namespace MR.Domain.Entities;

[Table(TableNames.QuarterIssues, Schema = SchemasNames.MRBasics)]
public class QuarterIssue
{
    [ForeignKey(nameof(Issue)), Key, Column(Order = 0)]
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }

    [ForeignKey(nameof(Quarter)), Key, Column(Order = 1)]
    public Guid QuarterId { get; set; }
    public Quarter Quarter { get; set; }
}