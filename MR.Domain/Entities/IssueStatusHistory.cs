namespace MR.Domain.Entities;

[Table(TableNames.IssueStatusHistories, Schema = SchemasNames.MRBasics)]
public class IssueStatusHistory : BaseEntity<Guid>
{
    public IssueStatus IssueStatus { get; set; }
    [ForeignKey(nameof(Issue))]
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }
}