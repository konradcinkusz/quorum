namespace MR.Domain.Entities;

[Table(TableNames.SignaturePools, Schema = SchemasNames.MRBasics)]
public class SignaturePool : BaseEntity<Guid>
{
    [ForeignKey(nameof(ApplicationUser))]
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
    [InverseProperty(nameof(Signature.SignaturePool))]
    public ICollection<Signature> Signatures { get; set; }
    [ForeignKey(nameof(Quarter))]
    public Guid QuarterId { get; set; }
    public Quarter Quarter { get; set; }
}
