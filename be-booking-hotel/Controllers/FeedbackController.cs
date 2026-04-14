using System.Security.Claims;
using be_booking_hotel.DTOs;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(
            IFeedbackService feedbackService,
            ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Lấy tất cả feedback của user hiện tại
        /// GET: api/feedbacks/my
        /// </summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyFeedbacks()
        {
            try
            {
                var feedbacks = await _feedbackService.GetMyFeedbacksAsync(GetUserId());
                return Ok(new
                {
                    success = true,
                    message = $"Found {feedbacks.Count} feedbacks",
                    data = feedbacks,
                    totalCount = feedbacks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my feedbacks");
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết một feedback (public)
        /// GET: api/feedbacks/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
                if (feedback == null)
                    return NotFound(new { success = false, message = $"Feedback with ID {id} not found" });

                return Ok(new { success = true, message = "Feedback retrieved successfully", data = feedback });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo feedback mới (cần PaymentStatus = Complete)
        /// POST: api/feedbacks
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
        {
            try
            {
                var (success, message, data) = await _feedbackService.CreateFeedbackAsync(GetUserId(), dto);

                if (!success)
                    return BadRequest(new { success = false, message });

                return CreatedAtAction(nameof(GetById), new { id = data!.FeedbackId }, new
                {
                    success = true,
                    message,
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback");
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật feedback (chỉ owner)
        /// PUT: api/feedbacks/{id}
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFeedbackDto dto)
        {
            try
            {
                var (success, message, data) = await _feedbackService.UpdateFeedbackAsync(GetUserId(), id, dto);

                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feedback {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa feedback (chỉ owner)
        /// DELETE: api/feedbacks/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, message) = await _feedbackService.DeleteFeedbackAsync(GetUserId(), id);

                if (!success)
                    return NotFound(new { success = false, message });

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }
    }
}