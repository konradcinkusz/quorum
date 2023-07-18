namespace MR.Service.FilesManagement;

public class UploadedFile
{
    public string FileName { get; set; }
    public byte[] FileContent { get; set; }
    public string PublicId { get; set; }
}
