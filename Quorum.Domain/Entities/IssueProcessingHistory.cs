namespace Quorum.Domain.Entities;

[Table(TableNames.IssueProcessingHistory, Schema = SchemasNames.MRBasics)]
public class IssueProcessingHistory : BaseEntity<Guid>
{
    public IssueProcess IssueProcess { get; set; }
    [ForeignKey(nameof(Issue))]
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }
}