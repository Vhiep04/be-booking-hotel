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
                AvailableRooms = h.Rooms.Count, // TODO: Calculate based on reservations
                CreatedAt = h.CreatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return (hotelDtos, totalCount, totalPages);
        }

        public async Task<HotelDetailDto?> GetHotelDetailAsync(int hotelId)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId);

            if (hotel == null)
                return null;

            var feedbacks = await _hotelRepository.GetHotelFeedbacksAsync(hotelId);
            var ratingDist = await _hotelRepository.GetRatingDistributionAsync(hotelId);

            var hotelDetail = new HotelDetailDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Location = hotel.Location,
                Description = hotel.Description,
                ImgUrl = hotel.ImgUrl,
                CreatedAt = hotel.CreatedAt,

                City = new CityDto
                {
                    CityId = hotel.City.CityId,
                    Name = hotel.City.Name,
                    Country = hotel.City.Country,
                    Description = hotel.City.Description,
                    Latitude = hotel.City.Latitude,
                    Longitude = hotel.City.Longitude
                },

                MinPrice = hotel.Rooms.Any() ? hotel.Rooms.Min(r => r.PricePerNight) : 0,
                MaxPrice = hotel.Rooms.Any() ? hotel.Rooms.Max(r => r.PricePerNight) : 0,

                AverageRating = hotel.Feedbacks.Any() ? hotel.Feedbacks.Average(f => f.Rating) : null,
                TotalReviews = hotel.Feedbacks.Count,

                RatingDistribution = new RatingDistribution
                {
                    FiveStar = ratingDist[5],
                    FourStar = ratingDist[4],
                    ThreeStar = ratingDist[3],
                    TwoStar = ratingDist[2],
                    OneStar = ratingDist[1]
                },

                TotalRooms = hotel.Rooms.Count,
                RoomTypes = hotel.Rooms.Select(r => r.RoomType).Distinct().ToList(),

                Facilities = hotel.Rooms
                    .SelectMany(r => r.Facilities.Select(f => f.Name))
                    .Distinct()
                    .ToList(),

                RecentFeedbacks = feedbacks.Select(f => new FeedbackDto
                {
                    FeedbackId = f.FeedbackId,
                    UserName = "Anonymous", // TODO: Get from User
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                }).ToList()
            };

            return hotelDetail;
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
                IsAvailable = true, // TODO: Check reservations
                BookedDays = 0 // TODO: Calculate from reservations
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
