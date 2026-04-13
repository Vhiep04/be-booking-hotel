
using be_booking_hotel.DTOs.Hotel;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace be_booking_hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavouritesController : ControllerBase
    {
        private readonly IFavouriteService _favouriteService;
        private readonly ILogger<FavouritesController> _logger;

        public FavouritesController(
            IFavouriteService favouriteService,
            ILogger<FavouritesController> logger)
        {
            _favouriteService = favouriteService;
            _logger = logger;
        }

        private string? GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetFavourites()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Unauthorized" });

                var favourites = await _favouriteService.GetFavouritesAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Favourites retrieved successfully",
                    data = favourites,
                    totalCount = favourites.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favourites");
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddFavourite([FromBody] AddFavouriteDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Unauthorized" });

                var (success, message, data) = await _favouriteService.AddFavouriteAsync(userId, dto.HotelId);

                if (!success)
                    return BadRequest(new { success = false, message });

                return CreatedAtAction(nameof(GetFavourites), new
                {
                    success = true,
                    message,
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding favourite");
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }

        [HttpDelete("{favouriteId}")]
        public async Task<IActionResult> DeleteFavourite(int favouriteId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { success = false, message = "Unauthorized" });

                var (success, message) = await _favouriteService.DeleteFavouriteAsync(favouriteId, userId);

                if (!success)
                    return NotFound(new { success = false, message });

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting favourite {FavouriteId}", favouriteId);
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }
    }
}
