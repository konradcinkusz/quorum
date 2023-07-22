namespace MR.Service.FilesManagement.Models;

public class UploadedFile
{
    public UploadedFile(string name, byte[] content)
    {
        Name = name;
        Content = content;
    }

    public string Name { get; set; }
    public byte[] Content { get; set; }
}
