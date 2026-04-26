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

        public async Task<CloudinaryFolderResult> GetImagesByFolderAsync(string folder, int maxResults = 100, string? nextCursor = null)
        {
            try
            {
                if (!AllowedFolders.Contains(folder.ToLower()))
                    return new CloudinaryFolderResult
                    {
                        Success = false,
                        Error = $"Invalid folder. Allowed: {string.Join(", ", AllowedFolders)}"
                    };

                folder = folder.ToLower();

                var listParams = new ListResourcesByPrefixParams
                {
                    Prefix = folder + "/",
                    Type = "upload",
                    MaxResults = maxResults,
                    NextCursor = nextCursor
                };

                var result = await _cloudinary.ListResourcesAsync(listParams);

                if (result.Resources == null || !result.Resources.Any())
                {
                    listParams.Prefix = folder;
                    listParams.NextCursor = null;
                    result = await _cloudinary.ListResourcesAsync(listParams);
                }

                if (result == null)
                    return new CloudinaryFolderResult { Success = false, Error = "No response from Cloudinary." };

                var images = result.Resources.Select(r => new CloudinaryImageInfo
                {
                    Url = r.SecureUrl.ToString(),
                    PublicId = r.PublicId,
                    Bytes = r.Bytes,
                    Width = r.Width,
                    Height = r.Height,
                }).ToList();

                return new CloudinaryFolderResult
                {
                    Success = true,
                    Images = images,
                    TotalCount = images.Count(),
                    NextCursor = result.NextCursor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Cloudinary folder listing for folder: {Folder}", folder);
                return new CloudinaryFolderResult { Success = false, Error = "Failed to retrieve images: " + ex.Message };
            }
        }
    }
}