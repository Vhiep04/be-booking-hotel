using be_booking_hotel.DTOs;
using be_booking_hotel.DTOs.Admin;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers
{
    /// <summary>
    /// Upload ảnh lên Cloudinary, trả về URL để dùng cho Create/Update City và Hotel.
    /// 
    /// FLOW:
    ///   1. POST /api/upload/image  →  { url, publicId }
    ///   2. Dùng url đó trong:
    ///      - POST /api/admin/cities/{id}/images      { imageUrl: url, ... }
    ///      - POST /api/admin/hotels/{id}/images      { imageUrl: url, ... }
    ///      - POST /api/admin/cities  (field: imageUrl trong body)
    ///      - POST /api/admin/hotels  (field: imageUrl trong body)
    /// </summary>
    [ApiController]
    [Route("api/upload")]
    [Authorize(Roles = "Admin,User,Manager")]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinary;

        public UploadController(ICloudinaryService cloudinary)
        {
            _cloudinary = cloudinary;
        }

        /// <summary>
        /// Upload 1 ảnh lên Cloudinary.
        /// Form-data: file (IFormFile), folder (string: "cities" | "hotels" | "rooms")
        /// </summary>
        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
        {
            var result = await _cloudinary.UploadImageAsync(request.File, request.Folder);

            if (!result.Success)
                return BadRequest(AdminApiResponse<UploadImageResponse>.Fail(result.Error ?? "Upload failed."));

            return Ok(AdminApiResponse<UploadImageResponse>.Ok(new UploadImageResponse
            {
                Url = result.Url!,
                PublicId = result.PublicId!
            }, "Image uploaded successfully."));
        }

        /// <summary>
        /// Upload nhiều ảnh cùng lúc (tối đa 10 ảnh).
        /// Form-data: files (List&lt;IFormFile&gt;), folder (string)
        /// </summary>
        [HttpPost("images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages([FromForm] UploadImagesRequest request)
        {
            var files = request.Files;

            if (files == null || files.Count == 0)
                return BadRequest(AdminApiResponse<object>.Fail("No files provided."));

            if (files.Count > 10)
                return BadRequest(AdminApiResponse<object>.Fail("Maximum 10 files allowed per upload."));

            var tasks = files.Select(f => _cloudinary.UploadImageAsync(f, request.Folder));
            var results = await Task.WhenAll(tasks);

            var succeeded = results.Where(r => r.Success).Select(r => new UploadImageResponse
            {
                Url = r.Url!,
                PublicId = r.PublicId!
            }).ToList();

            var failed = results.Count(r => !r.Success);

            return Ok(AdminApiResponse<UploadImagesResponse>.Ok(new UploadImagesResponse
            {
                Uploaded = succeeded,
                FailedCount = failed
            }, $"Uploaded {succeeded.Count}/{files.Count} images."));
        }

        /// <summary>
        /// Xóa ảnh khỏi Cloudinary theo publicId.
        /// </summary>
        [HttpDelete("image")]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest(AdminApiResponse<object>.Fail("publicId is required."));

            var deleted = await _cloudinary.DeleteImageAsync(publicId);
            return deleted
                ? Ok(AdminApiResponse<bool>.Ok(true, "Image deleted from Cloudinary."))
                : BadRequest(AdminApiResponse<bool>.Fail("Failed to delete image."));
        }

        /// <summary>
        /// Lấy tất cả ảnh trong một folder trên Cloudinary.
        /// Query: folder (string: "cities" | "hotels" | "rooms" | "avatars")
        ///        maxResults (int, default 100, max 500)
        ///        nextCursor (string, dùng để phân trang - lấy từ response trước)
        /// </summary>
        [HttpGet("images")]
        public async Task<IActionResult> GetImagesByFolder(
            [FromQuery] string folder,
            [FromQuery] int maxResults = 100,
            [FromQuery] string? nextCursor = null)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return BadRequest(AdminApiResponse<object>.Fail("folder is required."));

            if (maxResults < 1 || maxResults > 500)
                return BadRequest(AdminApiResponse<object>.Fail("maxResults must be between 1 and 500."));

            var result = await _cloudinary.GetImagesByFolderAsync(folder, maxResults, nextCursor);

            if (!result.Success)
                return BadRequest(AdminApiResponse<object>.Fail(result.Error ?? "Failed to retrieve images."));

            return Ok(AdminApiResponse<CloudinaryFolderResult>.Ok(result,
                $"Retrieved {result.Images.Count} images from folder '{folder}'."));
        }
    }
}
