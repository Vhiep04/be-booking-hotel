using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using be_booking_hotel.Services.Interfaces;
using be_booking_hotel.DTOs.Admin;

namespace be_booking_hotel.Services.Implements
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        private static readonly string[] AllowedFolders = { "cities", "hotels", "rooms", "avatars" };

        public CloudinaryService(IConfiguration config, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var cloudName = config["Cloudinary:CloudName"]
                ?? throw new InvalidOperationException("Cloudinary:CloudName is not configured.");
            var apiKey = config["Cloudinary:ApiKey"]
                ?? throw new InvalidOperationException("Cloudinary:ApiKey is not configured.");
            var apiSecret = config["Cloudinary:ApiSecret"]
                ?? throw new InvalidOperationException("Cloudinary:ApiSecret is not configured.");

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder)
        {
            try
            {
                if (!AllowedFolders.Contains(folder.ToLower()))
                    return new CloudinaryUploadResult
                    {
                        Success = false,
                        Error = $"Invalid folder. Allowed: {string.Join(", ", AllowedFolders)}"
                    };

                folder = folder.ToLower();

                if (file.Length == 0)
                    return new CloudinaryUploadResult { Success = false, Error = "File is empty." };

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return new CloudinaryUploadResult { Success = false, Error = "Invalid file type. Only JPEG, PNG, WEBP, GIF are allowed." };

                const long maxSize = 10 * 1024 * 1024;
                if (file.Length > maxSize)
                    return new CloudinaryUploadResult { Success = false, Error = "File size exceeds 10MB limit." };

                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                    PublicId = $"{folder}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    Overwrite = false
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
                    return new CloudinaryUploadResult { Success = false, Error = result.Error.Message };
                }

                return new CloudinaryUploadResult
                {
                    Success = true,
                    Url = result.SecureUrl.ToString(),
                    PublicId = result.PublicId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Cloudinary upload");
                return new CloudinaryUploadResult { Success = false, Error = "Upload failed: " + ex.Message };
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {
                var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Cloudinary delete for publicId: {PublicId}", publicId);
                return false;
            }
        }
    }
}