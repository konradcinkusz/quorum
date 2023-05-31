namespace MR.Shared.DTOs.Issue;

public class IssueAdminCreateDTO : IssueCreateDTO
{
    public string? ApplicationUserEmail { get; set; }
    //nullable
    public string ApplicationUserId { get; set; }
}
