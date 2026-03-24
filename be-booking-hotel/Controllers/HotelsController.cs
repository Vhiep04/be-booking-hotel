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
        /// Lấy chi tiết hotel theo ID
        /// </summary>
        /// <param name="id">Hotel ID</param>
        /// <returns>Hotel details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(int id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelDetailAsync(id);

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
                    data = hotel
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

        /// <summary>
        /// Lấy chi tiết 1 room của hotel
        /// </summary>
        /// <param name="id">Hotel ID</param>
        /// <param name="roomId">Room ID</param>
        /// <returns>Room detail</returns>
        [HttpGet("{id}/rooms/{roomId}")]
        public async Task<IActionResult> GetRoomById(int id, int roomId)
        {
            try
            {
                var room = await _hotelService.GetRoomByIdAsync(id, roomId);

                if (room == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Room with ID {roomId} not found in hotel {id}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Room details retrieved successfully",
                    data = room
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room {RoomId} for hotel {HotelId}", roomId, id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving room details",
                    error = ex.Message
                });
            }
        }
    }
}