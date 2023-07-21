namespace MR.Domain.Entities;

[Table(TableNames.CloudinaryFiles, Schema = SchemasNames.MRBasics)]
public class CloudinaryFile : BaseEntity<Guid>
{
    public string PublicId { get; set; }
    public string Url { get; set; }
    public string? Description { get; set; }

    [ForeignKey(nameof(Issue))]
    public Guid? IssueId { get; set; }
    public Issue? Issue { get; set; }
}
