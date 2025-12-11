using be_booking_hotel.DTOs.Hotel;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services.Implements
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly ILogger<HotelService> _logger;

        public HotelService(
            IHotelRepository hotelRepository,
            ILogger<HotelService> logger)
        {
            _hotelRepository = hotelRepository;
            _logger = logger;
        }

        public async Task<(List<HotelDto> Hotels, int TotalCount, int TotalPages)> GetHotelsAsync(HotelFilterDto filter)
        {
            var (hotels, totalCount) = await _hotelRepository.GetHotelsAsync(filter);

            var hotelDtos = hotels.Select(h => new HotelDto
            {
                HotelId = h.HotelId,
                Name = h.Name,
                Location = h.Location,
                Description = h.Description,
                ImgUrl = h.ImgUrl,
                CityId = h.CityId,
                CityName = h.City.Name,
                Country = h.City.Country,
                MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.PricePerNight) : 0,
                MaxPrice = h.Rooms.Any() ? h.Rooms.Max(r => r.PricePerNight) : 0,
                AverageRating = h.Feedbacks.Any() ? h.Feedbacks.Average(f => f.Rating) : null,
                TotalReviews = h.Feedbacks.Count,
                TotalRooms = h.Rooms.Count,
                AvailableRooms = h.Rooms.Count,
                CreatedAt = h.CreatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return (hotelDtos, totalCount, totalPages);
        }

        public async Task<HotelDetailDto?> GetHotelDetailAsync(int hotelId, RoomFilterDto filter)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId);

            if (hotel == null)
                return null;

            var feedbacks = await _hotelRepository.GetHotelFeedbacksAsync(hotelId);
            var ratingDist = await _hotelRepository.GetRatingDistributionAsync(hotelId);

            // ✅ Lấy số phòng đã đặt nếu có checkIn/checkOut
            Dictionary<int, int> bookedRoomsCount = new();
            if (filter.CheckIn.HasValue && filter.CheckOut.HasValue)
            {
                bookedRoomsCount = await _hotelRepository.GetBookedRoomsCountAsync(
                    hotelId,
                    filter.CheckIn.Value,
                    filter.CheckOut.Value
                );
            }

            // ✅ Tính average rating của hotel (dùng cho sort by rating)
            var hotelAvgRating = hotel.Feedbacks.Any() ? hotel.Feedbacks.Average(f => f.Rating) : 0;

            var hotelDetail = new HotelDetailDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Location = hotel.Location,
                Description = hotel.Description,
                ImgUrl = hotel.ImgUrl,

                City = new CityDto
                {
                    CityId = hotel.City.CityId,
                    Name = hotel.City.Name,
                    Country = hotel.City.Country,
                    Description = hotel.City.Description,
                    Latitude = hotel.City.Latitude,
                    Longitude = hotel.City.Longitude
                },

                AverageRating = hotelAvgRating > 0 ? hotelAvgRating : null,
                TotalReviews = hotel.Feedbacks.Count,

                RatingDistribution = new RatingDistribution
                {
                    FiveStar = ratingDist[5],
                    FourStar = ratingDist[4],
                    ThreeStar = ratingDist[3],
                    TwoStar = ratingDist[2],
                    OneStar = ratingDist[1]
                },

                HotelFacilities = hotel.Rooms
                    .SelectMany(r => r.Facilities.Select(f => f.Name))
                    .Distinct()
                    .ToList(),

                // ✅ FILTER & SORT ROOMS
                Rooms = GetFilteredAndSortedRooms(
                    hotel.Rooms,
                    bookedRoomsCount,
                    filter,
                    hotelAvgRating
                ),

                RecentFeedbacks = feedbacks.Select(f => new FeedbackDto
                {
                    FeedbackId = f.FeedbackId,
                    UserName = "Anonymous",
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                }).ToList()
            };

            return hotelDetail;
        }

        private List<string> GetBedTypes(string roomType, int capacity)
        {
            return roomType.ToLower() switch
            {
                var t when t.Contains("king") => new List<string> { "1 giường đôi lớn" },
                var t when t.Contains("double") => new List<string> { "1 giường đôi" },
                var t when t.Contains("twin") || t.Contains("two single") => new List<string> { "2 giường đơn" },
                var t when t.Contains("single") => new List<string> { "1 giường đơn" },
                var t when t.Contains("suite") => new List<string> { $"{capacity} người" },
                _ => new List<string> { $"{capacity} người" }
            };
        }

        /// <summary>
        /// Helper method để filter và sort rooms
        /// </summary>
        private List<RoomDetailDto> GetFilteredAndSortedRooms(
            ICollection<Models.Room> rooms,
            Dictionary<int, int> bookedRoomsCount,
            RoomFilterDto filter,
            double hotelAvgRating)
        {
            // Bước 1: Group và tạo RoomDetailDto
            var roomsWithFacilities = rooms
                .GroupBy(r => r.RoomType)
                .Select(g =>
                {
                    var firstRoom = g.First();
                    var totalRoomsOfType = g.Count();
                    var bookedCount = bookedRoomsCount.GetValueOrDefault(firstRoom.RoomId, 0);
                    var roomsLeft = totalRoomsOfType - bookedCount;

                    return new
                    {
                        Room = new RoomDetailDto
                        {
                            RoomId = firstRoom.RoomId,
                            RoomType = firstRoom.RoomType,
                            PricePerNight = firstRoom.PricePerNight,
                            OriginalPrice = firstRoom.PricePerNight * 1.2m,
                            Capacity = firstRoom.Capacity,
                            ImgUrl = firstRoom.ImgUrl,

                            Facilities = new RoomFacilitiesGroup
                            {
                                BedTypes = GetBedTypes(firstRoom.RoomType, firstRoom.Capacity),
                                RoomFeatures = firstRoom.Facilities
                                    .Where(f => new[] { "WiFi", "Air Conditioning", "TV", "Mini Bar", "Safe Box" }
                                        .Contains(f.Name))
                                    .Select(f => f.Name)
                                    .ToList(),
                                Bathroom = firstRoom.Facilities
                                    .Where(f => new[] { "Bathtub", "Own Bathroom", "Shower" }
                                        .Contains(f.Name))
                                    .Select(f => f.Name)
                                    .ToList(),
                                View = firstRoom.Facilities
                                    .Where(f => new[] { "Sea View", "City View", "Garden View" }
                                        .Contains(f.Name))
                                    .Select(f => f.Name)
                                    .ToList()
                            },

                            IsAvailable = roomsLeft > 0,
                            RoomsLeft = roomsLeft,

                            Conditions = new BookingConditions
                            {
                                FreeCancellation = firstRoom.Facilities.Any(f => f.Name == "Free Cancellation"),
                                CancellationDeadline = DateTime.Now.AddDays(2),
                                BreakfastIncluded = firstRoom.Facilities.Any(f => f.Name == "Breakfast Included"),
                                PrepaymentRequired = false
                            }
                        },
                        RoomFacilityNames = firstRoom.Facilities.Select(f => f.Name).ToList()
                    };
                })
                .ToList();

            // Bước 2: Apply filters
            var filteredRooms = roomsWithFacilities
                // FILTER 1: Bed Type
                .Where(x => string.IsNullOrEmpty(filter.BedType) ||
                           x.Room.RoomType.Contains(filter.BedType, StringComparison.OrdinalIgnoreCase))
                // FILTER 2: Price Range
                .Where(x => (!filter.MinPrice.HasValue || x.Room.PricePerNight >= filter.MinPrice.Value) &&
                           (!filter.MaxPrice.HasValue || x.Room.PricePerNight <= filter.MaxPrice.Value))
                // FILTER 3: Facilities
                .Where(x => filter.Facilities == null ||
                           !filter.Facilities.Any() ||
                           filter.Facilities.All(f => x.RoomFacilityNames.Contains(f, StringComparer.OrdinalIgnoreCase)))
                // FILTER 4: Availability
                .Where(x => !filter.CheckIn.HasValue || x.Room.IsAvailable)
                .Select(x => x.Room)
                .ToList();

            // Bước 3: Apply sorting
            return filter.SortBy?.ToLower() switch
            {
                "price_desc" => filteredRooms.OrderByDescending(r => r.PricePerNight).ToList(),
                "rating" => filteredRooms.OrderByDescending(r => hotelAvgRating).ToList(),
                _ => filteredRooms.OrderBy(r => r.PricePerNight).ToList() // default: price_asc
            };
        }

        public async Task<RoomListDto?> GetHotelRoomsAsync(int hotelId)
        {
            var hotelExists = await _hotelRepository.HotelExistsAsync(hotelId);
            if (!hotelExists)
                return null;

            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId);
            var rooms = await _hotelRepository.GetHotelRoomsAsync(hotelId);

            var roomDtos = rooms.Select(r => new RoomDto
            {
                RoomId = r.RoomId,
                HotelId = r.HotelId,
                RoomType = r.RoomType,
                PricePerNight = r.PricePerNight,
                Capacity = r.Capacity,
                ImgUrl = r.ImgUrl,
                Facilities = r.Facilities.Select(f => f.Name).ToList(),
                IsAvailable = true,
                BookedDays = 0
            }).ToList();

            var roomList = new RoomListDto
            {
                HotelId = hotelId,
                HotelName = hotel!.Name,
                Rooms = roomDtos,
                TotalRooms = roomDtos.Count,
                AvailableRooms = roomDtos.Count(r => r.IsAvailable),
                MinPrice = roomDtos.Any() ? roomDtos.Min(r => r.PricePerNight) : 0,
                MaxPrice = roomDtos.Any() ? roomDtos.Max(r => r.PricePerNight) : 0
            };

            return roomList;
        }
    }
}