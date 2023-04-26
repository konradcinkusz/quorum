namespace MR.Shared.DTOs.Admin;

public class AdminLogDTO
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Action { get; set; }
    public string Values { get; set; }
}
