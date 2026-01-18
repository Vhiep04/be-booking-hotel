using be_booking_hotel.DTOs;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _cityRepository;

        public CityService(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task<List<CityDto>> GetAllCitiesAsync()
        {
            var cities = await _cityRepository.GetAllAsync();
            return cities.Select(MapToCityDto).ToList();
        }

        public async Task<CityDto?> GetCityByIdAsync(int id)
        {
            var city = await _cityRepository.GetByIdAsync(id);
            return city == null ? null : MapToCityDto(city);
        }

        // ← GIỮ LẠI METHOD NÀY
        public async Task<List<CityDto>> SearchCitiesAsync(string searchTerm)
        {
            var cities = await _cityRepository.SearchByNameOrCountryAsync(searchTerm);
            return cities.Select(MapToCityDto).ToList();
        }

        /// <summary>
        /// Lấy TẤT CẢ hotels với filtering (không giới hạn city)
        /// </summary>
        public async Task<AllHotelsResultDto> GetAllHotelsWithFilterAsync(HotelFilterDto filter)
        {
            var hotels = await _cityRepository.GetAllHotelsAsync();
            var filteredHotels = ApplyFilters(hotels, filter);

            return new AllHotelsResultDto
            {
                Hotels = filteredHotels.Select(MapToHotelInCityDto).ToList()
            };
        }

        public async Task<CityHotelsResultDto> GetCityHotelsWithFilterAsync(int cityId, HotelFilterDto filter)
        {
            var cityExists = await _cityRepository.CityExistsAsync(cityId);
            if (!cityExists)
            {
                throw new KeyNotFoundException($"City with ID {cityId} not found");
            }

            var city = await _cityRepository.GetByIdAsync(cityId);
            var hotels = await _cityRepository.GetHotelsByCityIdAsync(cityId);

            var filteredHotels = ApplyFilters(hotels, filter);

            return new CityHotelsResultDto
            {
                CityName = city!.Name,
                Hotels = filteredHotels.Select(MapToHotelInCityDto).ToList()
            };
        }

        public async Task<HotelInCityDto?> GetHotelInCityAsync(int cityId, int hotelId)
        {
            var cityExists = await _cityRepository.CityExistsAsync(cityId);
            if (!cityExists)
            {
                throw new KeyNotFoundException($"City with ID {cityId} not found");
            }

            var hotel = await _cityRepository.GetHotelInCityAsync(cityId, hotelId);
            return hotel == null ? null : MapToHotelInCityDto(hotel);
        }

        public async Task<CityStatsDto?> GetCityStatsAsync(int cityId)
        {
            var city = await _cityRepository.GetByIdAsync(cityId);
            if (city == null) return null;

            var hotels = await _cityRepository.GetHotelsByCityIdAsync(cityId);
            var allRooms = hotels.SelectMany(h => h.Rooms).ToList();

            if (!allRooms.Any())
            {
                return new CityStatsDto
                {
                    CityId = cityId,
                    CityName = city.Name,
                    TotalHotels = 0,
                    TotalRooms = 0
                };
            }

            var roomTypeDistribution = allRooms
                .GroupBy(r => r.RoomType)
                .ToDictionary(g => g.Key, g => g.Count());

            var facilityCount = allRooms
                .SelectMany(r => r.Facilities)
                .GroupBy(f => new { f.FacilityId, f.Name })
                .Select(g => new PopularFacilityDto
                {
                    FacilityId = g.Key.FacilityId,
                    Name = g.Key.Name,
                    Count = g.Count()
                })
                .OrderByDescending(f => f.Count)
                .Take(10)
                .ToList();

            return new CityStatsDto
            {
                CityId = cityId,
                CityName = city.Name,
                TotalHotels = hotels.Count,
                TotalRooms = allRooms.Count,
                MinPricePerNight = allRooms.Min(r => r.PricePerNight),
                MaxPricePerNight = allRooms.Max(r => r.PricePerNight),
                AveragePricePerNight = allRooms.Average(r => r.PricePerNight),
                RoomTypeDistribution = roomTypeDistribution,
                PopularFacilities = facilityCount
            };
        }

        private List<Hotel> ApplyFilters(List<Hotel> hotels, HotelFilterDto filter)
        {
            var query = hotels.AsQueryable();

            // ✅ THÊM: Filter by CityName
            if (!string.IsNullOrWhiteSpace(filter.CityName))
            {
                query = query.Where(h => h.City != null &&
                                         h.City.Name.Contains(filter.CityName, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by date availability
            if (filter.CheckIn.HasValue && filter.CheckOut.HasValue)
            {
                query = query.Where(h => h.Rooms.Any(r =>
                    !r.Reservations.Any(res =>
                        filter.CheckIn.Value < res.CheckOutDate &&
                        filter.CheckOut.Value > res.CheckInDate
                    )
                ));
            }

            // Filter by bed type
            if (!string.IsNullOrWhiteSpace(filter.BedType))
            {
                query = query.Where(h => h.Rooms.Any(r => r.RoomType.Contains(filter.BedType)));
            }

            // Filter by price range
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(h => h.Rooms.Any(r => r.PricePerNight >= filter.MinPrice.Value));
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(h => h.Rooms.Any(r => r.PricePerNight <= filter.MaxPrice.Value));
            }

            // Filter by facilities
            if (filter.Facilities != null && filter.Facilities.Any())
            {
                query = query.Where(h => h.Rooms.Any(r =>
                    filter.Facilities.All(fId =>
                        r.Facilities.Any(f => f.FacilityId == fId)
                    )
                ));
            }

            var filteredHotels = query.ToList();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                filteredHotels = filter.SortBy.ToLower() switch
                {
                    "price_asc" => filteredHotels.OrderBy(h => h.Rooms.Min(r => r.PricePerNight)).ToList(),
                    "price_desc" => filteredHotels.OrderByDescending(h => h.Rooms.Min(r => r.PricePerNight)).ToList(),
                    "rating_desc" => filteredHotels.OrderByDescending(h =>
                        h.Feedbacks != null && h.Feedbacks.Any() ? h.Feedbacks.Average(f => f.Rating) : 0).ToList(),
                    "name_asc" => filteredHotels.OrderBy(h => h.Name).ToList(),
                    "name_desc" => filteredHotels.OrderByDescending(h => h.Name).ToList(),
                    _ => filteredHotels
                };
            }

            return filteredHotels;
        }
        private CityDto MapToCityDto(City city)
        {
            return new CityDto
            {
                CityId = city.CityId,
                Name = city.Name,
                Country = city.Country,
                Description = city.Description,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                Images = city.CityImages?.Select(img => new CityImageDto
                {
                    ImageId = img.ImageId,
                    ImageUrl = img.ImageUrl,
                    IsPrimary = img.IsPrimary ?? false,
                    DisplayOrder = img.DisplayOrder ?? 0,
                    Description = img.Description
                }).ToList() ?? new List<CityImageDto>()
            };
        }

        private HotelInCityDto MapToHotelInCityDto(Hotel hotel)
        {
            var rooms = hotel.Rooms?.ToList() ?? new List<Room>();

            var allFacilities = rooms
                .SelectMany(r => r.Facilities ?? new List<Facility>())
                .GroupBy(f => f.FacilityId)
                .Select(g => g.First())
                .ToList();

            var primaryImage = hotel.HotelImages?
                .FirstOrDefault(img => img.IsPrimary == true);

            return new HotelInCityDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Location = hotel.Location ?? string.Empty,
                Description = hotel.Description,
                MinPricePerNight = rooms.Any() ? rooms.Min(r => r.PricePerNight) : 0,
                MaxPricePerNight = rooms.Any() ? rooms.Max(r => r.PricePerNight) : 0,
                PrimaryImageUrl = primaryImage?.ImageUrl ?? hotel.ImgUrl,
                Images = hotel.HotelImages?.Select(img => new HotelImageDto
                {
                    ImageId = img.ImageId,
                    ImageUrl = img.ImageUrl,
                    IsPrimary = img.IsPrimary ?? false,
                    DisplayOrder = img.DisplayOrder ?? 0,
                    Description = img.Description
                }).ToList() ?? new List<HotelImageDto>(),
                AvailableRoomTypes = rooms.Select(r => r.RoomType).Distinct().ToList(),
                PopularFacilities = allFacilities.Select(f => new FacilityDto
                {
                    FacilityId = f.FacilityId,
                    Name = f.Name
                }).ToList(),
                AverageRating = hotel.Feedbacks != null && hotel.Feedbacks.Any()
                    ? hotel.Feedbacks.Average(f => f.Rating)
                    : null,
                TotalReviews = hotel.Feedbacks?.Count ?? 0
            };
        }
    }
}