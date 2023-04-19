namespace MR.Domain.Base;

public abstract class BaseEntity<Tkey> where Tkey : IEquatable<Tkey>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Tkey Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public abstract class BaseEntityLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Action { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public DateTime LogDate { get; set; }
}