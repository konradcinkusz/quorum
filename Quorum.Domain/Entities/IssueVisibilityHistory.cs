namespace Quorum.Domain.Entities;

[Table(TableNames.IssueVisibilityHistory, Schema = SchemasNames.MRBasics)]
public class IssueVisibilityHistory : BaseEntity<Guid>
{
    public IssueVisibility IssueVisibility { get; set; }
    [ForeignKey(nameof(Issue))]
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }
}
