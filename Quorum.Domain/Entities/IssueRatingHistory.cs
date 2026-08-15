namespace Quorum.Domain.Entities;

[Table(TableNames.IssueRatingHistory, Schema = SchemasNames.MRBasics)]
public class IssueRatingHistory : BaseEntity<int>
{
    [ForeignKey(nameof(Issue))]
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }

    public decimal Value { get; set; }
    public RatingAction Action { get; set; }
    public string? RelatedObject { get; set; }
}
