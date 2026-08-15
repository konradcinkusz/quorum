namespace Quorum.Domain.Entities.Base;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class BaseEntity<Tkey> : BaseEntity where Tkey : IEquatable<Tkey>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Tkey Id { get; set; }
}

public abstract class BaseEntityLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime LogDate { get; set; }
}
