using be_booking_hotel.DTOs.Admin;

namespace be_booking_hotel.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string publicId);
        Task<CloudinaryFolderResult> GetImagesByFolderAsync(string folder, int maxResults = 100, string? nextCursor = null);
    }
}
