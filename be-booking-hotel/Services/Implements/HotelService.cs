using be_booking_hotel.DTOs.Hotel;
using be_booking_hotel.Models;
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

                Latitude = h.Latitude,
                Longitude = h.Longitude,

                CityId = h.CityId,
                CityName = h.City.Name,
                Country = h.City.Country,
                MinPrice = h.RoomTypes.Any() ? h.RoomTypes.Min(r => r.PricePerNight) : 0,
                MaxPrice = h.RoomTypes.Any() ? h.RoomTypes.Max(r => r.PricePerNight) : 0,
                AverageRating = h.Feedbacks.Any() ? h.Feedbacks.Average(f => f.Rating) : null,
                TotalReviews = h.Feedbacks.Count,
                TotalRooms = h.RoomTypes.Count,
                AvailableRooms = h.RoomTypes.Count, // TODO: Calculate based on reservations
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

                Latitude = hotel.Latitude,
                Longitude = hotel.Longitude,

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

                MinPrice = hotel.RoomTypes.Any() ? hotel.RoomTypes.Min(r => r.PricePerNight) : 0,
                MaxPrice = hotel.RoomTypes.Any() ? hotel.RoomTypes.Max(r => r.PricePerNight) : 0,

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

                TotalRooms = hotel.RoomTypes.Count,
                RoomTypes = hotel.RoomTypes.Select(rt => new RoomTypeDto
                {
                    RoomTypeId = rt.RoomTypeId,
                    TypeName = rt.TypeName,
                    PricePerNight = rt.PricePerNight,
                    Capacity = rt.Capacity,
                    ImgUrl = rt.ImgUrl
                }).ToList(),

                Facilities = hotel.RoomTypes
                    .SelectMany(r => r.Facilities.Select(f => f.Name))
                    .Distinct()
                    .ToList(),

                RecentFeedbacks = feedbacks.Select(f => new FeedbackDto
                {
                    FeedbackId = f.FeedbackId,
                    UserName = "Anonymous",
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                }).ToList(),
                Images = hotel.HotelImages
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new HotelImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary ?? false,
                    DisplayOrder = i.DisplayOrder ?? 0
                }).ToList(),
            };

            return hotelDetail;
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int hotelId, int roomId)
        {
            var hotelExists = await _hotelRepository.HotelExistsAsync(hotelId);
            if (!hotelExists)
                return null;

            var room = await _hotelRepository.GetRoomByIdAsync(hotelId, roomId);
            if (room == null)
                return null;

            return new RoomDto
            {
                RoomId = room.RoomId,
                HotelId = room.HotelId,

                RoomType = room.RoomType?.TypeName ?? "",
                PricePerNight = room.RoomType?.PricePerNight ?? 0,
                Capacity = room.RoomType?.Capacity ?? 0,
                ImgUrl = room.RoomType?.ImgUrl,
                Facilities = room.RoomType?.Facilities?.Select(f => f.Name).ToList() ?? new(),

                IsAvailable = true,
                BookedDays = 0
            };
        }

        public async Task<RoomListDto?> GetHotelRoomsAsync(
        int hotelId,
        DateOnly? checkIn = null,
        DateOnly? checkOut = null,
        string? roomType = null)
        {
            var hotelExists = await _hotelRepository.HotelExistsAsync(hotelId);
            if (!hotelExists)
                return null;

            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId);
            var rooms = await _hotelRepository.GetHotelRoomsAsync(hotelId, checkIn, checkOut, roomType);

            var roomDtos = rooms.Select(r =>
            {
                bool isAvailable = IsRoomAvailable(r, checkIn, checkOut);

                return new RoomDto
                {
                    RoomId = r.RoomId,
                    HotelId = r.HotelId,
                    RoomType = r.RoomType?.TypeName ?? "",
                    PricePerNight = r.RoomType?.PricePerNight ?? 0,
                    Capacity = r.RoomType?.Capacity ?? 0,
                    ImgUrl = r.RoomType?.ImgUrl,
                    Facilities = r.RoomType?.Facilities?.Select(f => f.Name).ToList() ?? new(),
                    IsAvailable = isAvailable,
                    BookedDays = r.Reservations?.Count(res =>
                        res.PaymentStatus != "Cancelled") ?? 0
                };
            }).ToList();

            return new RoomListDto
            {
                HotelId = hotelId,
                HotelName = hotel!.Name,
                HotelLocation = hotel.Location,
                HotelDescription = hotel.Description,
                HotelLatitude = hotel.Latitude,
                HotelLongitude = hotel.Longitude,
                Rooms = roomDtos,
                TotalRooms = roomDtos.Count,
                AvailableRooms = roomDtos.Count(r => r.IsAvailable),
                MinPrice = roomDtos.Any() ? roomDtos.Min(r => r.PricePerNight) : 0,
                MaxPrice = roomDtos.Any() ? roomDtos.Max(r => r.PricePerNight) : 0
            };
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int hotelId, int roomId, DateOnly? checkIn = null, DateOnly? checkOut = null)
        {
            var hotelExists = await _hotelRepository.HotelExistsAsync(hotelId);
            if (!hotelExists)
                return null;

            var room = await _hotelRepository.GetRoomByIdAsync(hotelId, roomId);
            if (room == null)
                return null;

            return new RoomDto
            {
                RoomId = room.RoomId,
                HotelId = room.HotelId,
                RoomType = room.RoomType?.TypeName ?? "",
                PricePerNight = room.RoomType?.PricePerNight ?? 0,
                Capacity = room.RoomType?.Capacity ?? 0,
                ImgUrl = room.RoomType?.ImgUrl,
                Facilities = room.RoomType?.Facilities?.Select(f => f.Name).ToList() ?? new(),
                IsAvailable = IsRoomAvailable(room, checkIn, checkOut),
                BookedDays = room.Reservations?.Count(res =>
                    res.PaymentStatus != "Cancelled") ?? 0
            };
        }

        // ✅ Helper: tính availability
        private static bool IsRoomAvailable(Room room, DateOnly? checkIn, DateOnly? checkOut)
        {
            // Nếu phòng đang bảo trì → không available bất kể ngày
            if (room.Status == "Maintenance")
                return false;

            // Nếu có truyền ngày → check overlap reservation (bỏ qua Status)
            if (checkIn.HasValue && checkOut.HasValue)
            {
                bool hasConflict = room.Reservations?.Any(res =>
                    res.PaymentStatus != "Cancelled" &&
                    checkIn.Value < res.CheckOutDate &&
                    checkOut.Value > res.CheckInDate
                ) ?? false;

                return !hasConflict;
            }

            // Không truyền ngày → dùng Status thuần
            return room.Status == "Available";
        }
    }
}