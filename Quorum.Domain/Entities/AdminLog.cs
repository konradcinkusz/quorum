namespace Quorum.Domain.Entities;

[Table(nameof(TableNames.Admin_Logs), Schema = SchemasNames.MRBasics)]
public class AdminLog : BaseEntity<int>
{
    public string? Action { get; set; }
    public string? Values { get; set; }
}
