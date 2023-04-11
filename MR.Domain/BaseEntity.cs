namespace MR.Domain.Base;

public abstract class BaseEntity<Tkey> where Tkey : IEquatable<Tkey>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Tkey Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
