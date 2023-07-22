namespace MR.Domain.Entities;

[Table(TableNames.CloudinaryFiles, Schema = SchemasNames.MRBasics)]
public class CloudinaryFile : BaseEntity<Guid>
{
    public string PublicId { get; set; }
    public string SecureUri { get; set; }
    public string FileName { get; set; }

    [InverseProperty(nameof(CloudinaryFileIssue.CloudinaryFile))]
    public ICollection<CloudinaryFileIssue> CloudinaryFileIssues { get; set; }
}
