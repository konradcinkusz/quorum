using CloudinaryDotNet.Actions;

namespace MR.Service.FilesManagement.Models;

public class ImageData : FileData
{
    public ImageData(RawUploadResult uploadResult, string fileName) : base(uploadResult, fileName)
    {
    }

    public string TransformedUrl { get; set; }
}
