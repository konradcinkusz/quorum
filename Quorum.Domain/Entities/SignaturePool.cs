namespace Quorum.Domain.Entities;

[Table(TableNames.SignaturePools, Schema = SchemasNames.MRBasics)]
public class SignaturePool : BaseEntity<Guid>
{
    /// <summary>Subject id from the identity service; no navigation on purpose (ADR 0001).</summary>
    public string ApplicationUserId { get; set; }

    /// <summary>Display email resolved from the <see cref="QuorumUser"/> projection by the
    /// query layer; not a column, and never authoritative.</summary>
    [NotMapped]
    public string? ApplicationUserEmail { get; set; }
    [InverseProperty(nameof(Signature.SignaturePool))]
    public ICollection<Signature> Signatures { get; set; }
    [ForeignKey(nameof(Quarter))]
    public Guid QuarterId { get; set; }
    public Quarter Quarter { get; set; }
}
