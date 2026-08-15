using CloudinaryDotNet.Actions;

namespace Quorum.Service.FilesManagement.Models;

public class FileData
{
    public FileData(RawUploadResult uploadResult, string fileName)
    {
        SecureUri = uploadResult.SecureUri;
        PublicId = uploadResult.PublicId;
        FileName = fileName;
    }

    public string FileName { get; set; }
    public string PublicId { get; }
    public Uri SecureUri { get; }
}