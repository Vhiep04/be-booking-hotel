using be_booking_hotel.DTOs.Hotel;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(
            IHotelService hotelService,
            ILogger<HotelsController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách hotels với filter và pagination
        /// </summary>
        /// <param name="filter">Filter parameters</param>
        /// <returns>Paginated list of hotels</returns>
        [HttpGet]
        public async Task<IActionResult> GetHotels([FromQuery] HotelFilterDto filter)
        {
            try
            {
                var (hotels, totalCount, totalPages) = await _hotelService.GetHotelsAsync(filter);

                return Ok(new
                {
                    success = true,
                    message = "Hotels retrieved successfully",
                    data = hotels,
                    pagination = new
                    {
                        currentPage = filter.PageNumber,
                        pageSize = filter.PageSize,
                        totalCount,
                        totalPages,
                        hasNextPage = filter.PageNumber < totalPages,
                        hasPreviousPage = filter.PageNumber > 1
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotels");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving hotels",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết hotel theo ID với room filtering
        /// </summary>
        /// <param name="id">Hotel ID</param>
        /// <param name="filter">Room filter parameters (dates, facilities, sort)</param>
        /// <returns>Hotel details with filtered rooms</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(
            int id,
            [FromQuery] RoomFilterDto filter)
        {
            try
            {
                // Validate dates nếu có
                if (filter.CheckIn.HasValue && filter.CheckOut.HasValue)
                {
                    if (filter.CheckOut <= filter.CheckIn)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Check-out date must be after check-in date"
                        });
                    }

                    if (filter.CheckIn < DateOnly.FromDateTime(DateTime.Today))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Check-in date cannot be in the past"
                        });
                    }
                }

                var hotel = await _hotelService.GetHotelDetailAsync(id, filter);

                if (hotel == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Hotel with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Hotel details retrieved successfully",
                    data = hotel,
                    filters = new
                    {
                        appliedFacilities = filter.Facilities ?? new List<string>(),
                        bedType = filter.BedType,
                        sortBy = filter.SortBy,
                        totalRoomsFound = hotel.Rooms.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel {HotelId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving hotel details",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách rooms của hotel
        /// </summary>
        /// <param name="id">Hotel ID</param>
        /// <returns>List of rooms</returns>
        [HttpGet("{id}/rooms")]
        public async Task<IActionResult> GetHotelRooms(int id)
        {
            try
            {
                var rooms = await _hotelService.GetHotelRoomsAsync(id);

                if (rooms == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Hotel with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Hotel rooms retrieved successfully",
                    data = rooms
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms for hotel {HotelId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving hotel rooms",
                    error = ex.Message
                });
            }
        }
    }
}