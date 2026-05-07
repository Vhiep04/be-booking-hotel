using be_booking_hotel.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace be_booking_hotel.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repo;

        public NotificationController(INotificationRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            return Ok(await _repo.GetByUserIdAsync(userId));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            return Ok(await _repo.GetUnreadCountAsync(userId));
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _repo.MarkAsReadAsync(id);
            return NoContent();
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            await _repo.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminNotifications()
            => Ok(await _repo.GetAdminNotificationsAsync());

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/unread-count")]
        public async Task<IActionResult> GetAdminUnreadCount()
            => Ok(await _repo.GetAdminUnreadCountAsync());

        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/read-all")]
        public async Task<IActionResult> MarkAllAdminAsRead()
        {
            await _repo.MarkAllAdminAsReadAsync();
            return NoContent();
        }
    }
}