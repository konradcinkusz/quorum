using CloudinaryDotNet.Actions;

namespace Quorum.Service.FilesManagement.Models;

public class ImageData : FileData
{
    public ImageData(RawUploadResult uploadResult, string fileName) : base(uploadResult, fileName)
    {
    }

    public string TransformedUrl { get; set; }
}
