namespace MR.Service.FilesManagement
{
    //Nie wywalamy do global, ponieważ overriduje Expression z biblioteki Linq
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;

    internal interface ICloudinaryService
    {
        Task<ImageData> UploadImage(UploadedFile uploadedFile);
        Task<FileData> UploadPdf(UploadedFile uploadedFile);
    }

    internal class CloudinaryService : ICloudinaryService
    {
        readonly Cloudinary cloudinary;
        public CloudinaryService(IOptions<CloudinaryOpt> options)
        {
            Account account = new Account(options.Value.Cloud, options.Value.ApiKey, options.Value.ApiSecret);
            cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;
        }

        public async Task<ImageData> UploadImage(UploadedFile uploadedFile)
        {
            MemoryStream ms = new MemoryStream(uploadedFile.Content);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(uploadedFile.Name, ms)
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            return new(uploadResult, uploadedFile.Name)
            {
                TransformedUrl = cloudinary.Api.UrlImgUp.Transform(new Transformation().Width(150).Crop("scale")).Secure(true).BuildUrl(uploadResult.PublicId)
            };
        }

        public async Task<FileData> UploadPdf(UploadedFile uploadedFile)
        {
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(uploadedFile.Name, new MemoryStream(uploadedFile.Content)),
                    Tags = "pdf_file" // Optional tags to associate with the uploaded file
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception(uploadResult.Error.Message);
                }

                return new(uploadResult, uploadedFile.Name);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}