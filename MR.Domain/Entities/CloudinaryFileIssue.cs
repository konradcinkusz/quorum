namespace MR.Domain.Entities;

[Table(TableNames.CloudinaryFileIssues, Schema = SchemasNames.MRBasics)]
public class CloudinaryFileIssue
{
    [ForeignKey(nameof(Issue)), Key, Column(Order = 0)]
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; }

    [ForeignKey(nameof(CloudinaryFile)), Key, Column(Order = 1)]
    public Guid CloudinaryFileId { get; set; }
    public CloudinaryFile CloudinaryFile { get; set; }

    [ForeignKey(nameof(ApplicationUser))]
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public CloudinaryFileIssueType CloudinaryFileIssueType { get; set; }
}
