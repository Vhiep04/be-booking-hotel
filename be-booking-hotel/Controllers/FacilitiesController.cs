using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilitiesController : ControllerBase
    {
        private readonly IFacilityService _facilityService;
        private readonly ILogger<FacilitiesController> _logger;

        public FacilitiesController(
            IFacilityService facilityService,
            ILogger<FacilitiesController> logger)
        {
            _facilityService = facilityService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả facilities (dùng cho checkboxes)
        /// GET: api/facilities
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllFacilities()
        {
            try
            {
                var facilities = await _facilityService.GetAllFacilitiesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Facilities retrieved successfully",
                    data = facilities,
                    totalCount = facilities.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all facilities");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving facilities",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy facility theo ID
        /// GET: api/facilities/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFacilityById(int id)
        {
            try
            {
                var facility = await _facilityService.GetFacilityByIdAsync(id);
                if (facility == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Facility with ID {id} not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Facility retrieved successfully",
                    data = facility
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility {FacilityId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving facility",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy facilities của một room
        /// GET: api/facilities/room/{roomId}
        /// </summary>
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetRoomFacilities(int roomId)
        {
            try
            {
                var facilities = await _facilityService.GetRoomFacilitiesAsync(roomId);
                return Ok(new
                {
                    success = true,
                    message = "Room facilities retrieved successfully",
                    data = facilities,
                    roomId,
                    totalCount = facilities.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facilities for room {RoomId}", roomId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving room facilities",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Search facilities theo tên
        /// GET: api/facilities/search?term=wifi
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchFacilities([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Search term is required"
                    });
                }

                var facilities = await _facilityService.SearchFacilitiesAsync(term);
                return Ok(new
                {
                    success = true,
                    message = $"Found {facilities.Count} facilities matching '{term}'",
                    data = facilities,
                    searchTerm = term
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching facilities with term '{Term}'", term);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while searching facilities",
                    error = ex.Message
                });
            }
        }
    }
}