namespace Quorum.Service.FilesManagement
{
    //Nie wywalamy do global, ponieważ overriduje Expression z biblioteki Linq
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;

    internal interface ICloudinaryService
    {
        Task<ImageData> UploadImageAsync(UploadedFile uploadedFile, CancellationToken cancellationToken);
        Task<FileData> UploadPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken);

        /// <summary>
        /// Stores a signed petition sheet so that its delivery URL is <b>not</b> publicly
        /// fetchable, and can only be retrieved through <see cref="BuildSignedDownloadUrl"/>.
        /// </summary>
        /// <remarks>
        /// Separate from <see cref="UploadPdfAsync"/> on purpose rather than being a flag on
        /// it. The two carry different documents: <c>UploadPdfAsync</c> stores the blank sheet
        /// an administrator generates for people to print, which is public by design, and this
        /// stores the one that comes back with real names and signatures on it. A boolean
        /// parameter would make the safe choice something a new call site has to remember.
        /// </remarks>
        Task<FileData> UploadSignedPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken);

        /// <summary>
        /// A time-limited URL for a document stored by <see cref="UploadSignedPdfAsync"/>.
        /// </summary>
        /// <param name="lifetime">
        /// How long the URL remains valid. Short by intent: the URL is the credential once it
        /// has been handed out, and this bounds how long a copy in a browser history, a proxy
        /// log or a referrer header is worth anything.
        /// </param>
        string BuildSignedDownloadUrl(string publicId, TimeSpan lifetime);
    }

    internal class CloudinaryService : ICloudinaryService
    {
        /// <summary>
        /// Cloudinary's name for "not served without a signature". Its counterpart, and the
        /// default, is <c>upload</c>, which is public.
        /// </summary>
        internal const string AuthenticatedDeliveryType = "authenticated";

        readonly Cloudinary cloudinary;
        public CloudinaryService(IOptions<CloudinaryOpt> options)
        {
            Account account = new Account(options.Value.Cloud, options.Value.ApiKey, options.Value.ApiSecret);
            cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;
        }

        public async Task<ImageData> UploadImageAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
        {
            MemoryStream ms = new MemoryStream(uploadedFile.Content);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(uploadedFile.Name, ms)
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);

            return new(uploadResult, uploadedFile.Name)
            {
                TransformedUrl = cloudinary.Api.UrlImgUp.Transform(new Transformation().Width(150).Crop("scale")).Secure(true).BuildUrl(uploadResult.PublicId)
            };
        }

        public async Task<FileData> UploadPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(uploadedFile.Name, new MemoryStream(uploadedFile.Content)),
                Tags = "pdf_file" // Optional tags to associate with the uploaded file
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return new(uploadResult, uploadedFile.Name);
        }

        public async Task<FileData> UploadSignedPdfAsync(UploadedFile uploadedFile, CancellationToken cancellationToken)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(uploadedFile.Name, new MemoryStream(uploadedFile.Content)),
                Tags = "pdf_file,signed_document",

                // The whole point of this method. Cloudinary's own SDK documents this
                // property as "privacy mode of the file. Valid values: 'upload' and
                // 'authenticated'. Default: 'upload'." Under the default, the returned URL
                // is world-readable on the CDN and the unguessable file name is the only
                // thing standing between a stranger and a page of real signatures -- which
                // makes it a share token rather than an access control.
                Type = AuthenticatedDeliveryType,
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return new(uploadResult, uploadedFile.Name);
        }

        public string BuildSignedDownloadUrl(string publicId, TimeSpan lifetime)
        {
            // Arguments are positional because the overload takes six and naming a subset
            // reads as though the rest were defaulted deliberately somewhere:
            // (publicId, attachment, format, type, expiresAt, resourceType).
            //
            // attachment: true so a browser saves the sheet rather than rendering it inline,
            // which keeps it out of the tab history as a viewed document.
            // resourceType "image" because the upload path uses ImageUploadParams; a PDF
            // stored through it is an image resource as far as Cloudinary is concerned.
            return cloudinary.DownloadPrivate(
                publicId,
                true,
                "pdf",
                AuthenticatedDeliveryType,
                DateTimeOffset.UtcNow.Add(lifetime).ToUnixTimeSeconds(),
                "image");
        }
    }
}