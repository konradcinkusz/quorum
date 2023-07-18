namespace MR.Service.FilesManagement
{
    //Nie wywalamy do global, ponieważ overriduje Expression z biblioteki Linq
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;

    public interface ICloudinaryService
    {
        string GetPdfUrl(string publicId);
        Task<ImageData> UploadImage(UploadedFile uploadedFile);
        string UploadPdf(string filePath, string publicId);
    }

    internal class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary cloudinary;

        public CloudinaryService(IOptions<CloudinaryOpt> options)
        {
            Account account = new Account(
                               options.Value.Cloud,
                               options.Value.ApiKey,
                               options.Value.ApiSecret);

            cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;
        }

        public async Task<ImageData> UploadImage(UploadedFile uploadedFile)
        {
            MemoryStream ms = new MemoryStream(uploadedFile.FileContent);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(uploadedFile.FileName, ms)
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            return new()
            {
                DateAdded = DateTime.Now,
                Description = uploadedFile.FileName,
                IsMain = true,
                Public_Id = uploadResult.PublicId,
                Url = cloudinary.Api.UrlImgUp.Transform(new Transformation().Width(150).Crop("scale")).Secure(true).BuildUrl(uploadResult.PublicId)
            };
        }

        public string UploadPdf(string filePath, string publicId)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(filePath),
                PublicId = publicId,
                RawConvert = "pdf",
                Tags = "pdf_file" // Optional tags to associate with the uploaded file
            };

            var uploadResult = cloudinary.Upload(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return uploadResult.SecureUrl.AbsoluteUri;
        }

        public string GetPdfUrl(string publicId)
        {
            var getParams = new GetResourceParams(publicId)
            {
                ResourceType = ResourceType.Raw
            };

            var getResult = cloudinary.GetResource(getParams);

            if (getResult.Error != null)
            {
                throw new Exception(getResult.Error.Message);
            }

            return getResult.SecureUrl;
        }

    }
}