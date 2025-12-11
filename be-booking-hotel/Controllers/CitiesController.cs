using be_booking_hotel.Services.Interfaces;
using be_booking_hotel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _cityService;
        private readonly ILogger<CitiesController> _logger;

        public CitiesController(
            ICityService cityService,
            ILogger<CitiesController> logger)
        {
            _cityService = cityService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả cities hoặc search theo tên/quốc gia
        /// GET: api/cities
        /// GET: api/cities?name=hanoi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCities([FromQuery] string? name)
        {
            try
            {
                List<CityDto> cities;

                // Nếu có search term thì search, không thì lấy tất cả
                if (!string.IsNullOrWhiteSpace(name))
                {
                    cities = await _cityService.SearchCitiesAsync(name);
                    return Ok(new
                    {
                        success = true,
                        message = $"Found {cities.Count} cities matching '{name}'",
                        data = cities,
                        searchTerm = name,
                        totalCount = cities.Count
                    });
                }
                else
                {
                    cities = await _cityService.GetAllCitiesAsync();
                    return Ok(new
                    {
                        success = true,
                        message = "Cities retrieved successfully",
                        data = cities,
                        totalCount = cities.Count
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cities");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving cities",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy city theo ID (bao gồm images)
        /// GET: api/cities/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCityById(int id)
        {
            try
            {
                var city = await _cityService.GetCityByIdAsync(id);
                if (city == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"City with ID {id} not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "City retrieved successfully",
                    data = city
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting city {CityId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving city",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách hotels (tất cả hoặc theo city) với filtering
        /// GET: api/cities/hotels (lấy tất cả hotels)
        /// GET: api/cities/hotels?cityId=1 (lấy hotels của city cụ thể)
        /// GET: api/cities/hotels?cityId=1&checkIn=2024-01-01&checkOut=2024-01-05&minPrice=500000&maxPrice=5000000&bedType=King%20Bed&facilities=1,2,3&sortBy=price_asc
        /// </summary>
        [HttpGet("hotels")]
        public async Task<IActionResult> GetHotels(
            [FromQuery] int? cityId,
            [FromQuery] HotelFilterDto filter)
        {
            try
            {
                // Nếu không có cityId, lấy tất cả hotels (có thể implement logic này trong service)
                //if (!cityId.HasValue)
                //{
                //    // Tùy vào logic của bạn, có thể return tất cả hotels hoặc báo lỗi
                //    return BadRequest(new
                //    {
                //        success = false,
                //        message = "Please provide a cityId parameter"
                //    });
                //}

                var result = await _cityService.GetCityHotelsWithFilterAsync(cityId.Value, filter);

                if (result.Hotels == null || !result.Hotels.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No hotels found matching the criteria",
                        data = new
                        {
                            cityId = cityId.Value,
                            cityName = result.CityName,
                            hotels = new List<object>(),
                            filters = filter,
                            totalCount = 0
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Found {result.Hotels.Count} hotels in {result.CityName}",
                    data = new
                    {
                        cityId = cityId.Value,
                        cityName = result.CityName,
                        hotels = result.Hotels,
                        filters = filter,
                        totalCount = result.Hotels.Count
                    }
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotels with filters");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving hotels",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy hotel cụ thể trong city
        /// GET: api/cities/{cityId}/hotels/{hotelId}
        /// </summary>
        [HttpGet("{cityId}/hotels/{hotelId}")]
        public async Task<IActionResult> GetHotelInCity(int cityId, int hotelId)
        {
            try
            {
                var hotel = await _cityService.GetHotelInCityAsync(cityId, hotelId);

                if (hotel == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Hotel with ID {hotelId} not found in city {cityId}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Hotel retrieved successfully",
                    data = hotel
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel {HotelId} in city {CityId}", hotelId, cityId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving hotel",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thống kê về city (số hotels, price range, etc.)
        /// GET: api/cities/{id}/stats
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetCityStats(int id)
        {
            try
            {
                var stats = await _cityService.GetCityStatsAsync(id);

                if (stats == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"City with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "City statistics retrieved successfully",
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for city {CityId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving city statistics",
                    error = ex.Message
                });
            }
        }
    }
}