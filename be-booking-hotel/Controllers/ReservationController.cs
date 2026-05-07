using System.Security.Claims;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IReservationService reservationService,
            ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Lấy tất cả reservations của user đang đăng nhập
        /// GET: api/reservations
        /// GET: api/reservations?status=Confirmed
        /// GET: api/reservations?sort=oldest
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyReservations(
            [FromQuery] string? status = null,
            [FromQuery] string? sort = null)
        {
            try
            {
                var reservations = await _reservationService.GetMyReservationsAsync(
                    GetUserId(), status, sort);

                return Ok(new
                {
                    success = true,
                    message = $"Found {reservations.Count} reservations",
                    data = reservations,
                    totalCount = reservations.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservations for user");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving reservations",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết một reservation
        /// GET: api/reservations/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var reservation = await _reservationService.GetMyReservationDetailAsync(
                    id, GetUserId());

                if (reservation == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Reservation with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Reservation retrieved successfully",
                    data = reservation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving reservation",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// User tự huỷ reservation của mình
        /// PATCH: api/reservations/{id}/cancel
        /// </summary>
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var (success, message) = await _reservationService.CancelReservationAsync(
                    id, GetUserId());

                if (!success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while cancelling reservation",
                    error = ex.Message
                });
            }
        }
    }
}