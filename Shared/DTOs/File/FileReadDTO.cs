namespace Quorum.Shared.DTOs.CloudinaryFile;

public class FileReadDTO : BaseDTO
{
    public string PublicId { get; set; }
    public string SecureUri { get; set; }
    public string FileName { get; set; }
}