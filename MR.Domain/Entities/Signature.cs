namespace MR.Domain.Entities;

[Table(TableNames.Signatures, Schema = SchemasNames.MRBasics)]
public class Signature : BaseEntity<Guid>
{
    //CurrentSignedIssue
    [ForeignKey(nameof(Issue))]
    public Guid? IssueId { get; set; }
    public Issue? Issue { get; set; }

    [ForeignKey(nameof(SignaturePool))]
    public Guid SignaturePoolId { get; set; }
    public SignaturePool SignaturePool { get; set; }
}
