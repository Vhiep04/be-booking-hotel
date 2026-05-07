using be_booking_hotel.DTOs.Hotel;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services.Implements
{
    public class FavouriteService : IFavouriteService
    {
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly ILogger<FavouriteService> _logger;

        public FavouriteService(
            IFavouriteRepository favouriteRepository,
            IHotelRepository hotelRepository,
            ILogger<FavouriteService> logger)
        {
            _favouriteRepository = favouriteRepository;
            _hotelRepository = hotelRepository;
            _logger = logger;
        }

        public async Task<List<FavouriteDto>> GetFavouritesAsync(string userId)
        {
            var favourites = await _favouriteRepository.GetFavouritesByUserIdAsync(userId);

            return favourites.Select(f => new FavouriteDto
            {
                FavouriteId = f.FavouriteId,
                HotelId = f.HotelId,
                HotelName = f.Hotel.Name,
                HotelLocation = f.Hotel.Location,
                HotelImgUrl = f.Hotel.ImgUrl,
                CityName = f.Hotel.City.Name,
                Country = f.Hotel.City.Country,
                MinPrice = f.Hotel.RoomTypes.Any() ? f.Hotel.RoomTypes.Min(r => r.PricePerNight) : 0,
                MaxPrice = f.Hotel.RoomTypes.Any() ? f.Hotel.RoomTypes.Max(r => r.PricePerNight) : 0,
                AverageRating = f.Hotel.Feedbacks.Any() ? f.Hotel.Feedbacks.Average(fb => fb.Rating) : null,
                TotalReviews = f.Hotel.Feedbacks.Count,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        public async Task<(bool Success, string Message, FavouriteDto? Data)> AddFavouriteAsync(string userId, int hotelId)
        {
            // Check hotel tồn tại
            var hotelExists = await _hotelRepository.HotelExistsAsync(hotelId);
            if (!hotelExists)
                return (false, $"Hotel with ID {hotelId} not found", null);

            // Check đã favourite chưa
            var alreadyExists = await _favouriteRepository.IsFavouriteExistsAsync(userId, hotelId);
            if (alreadyExists)
                return (false, "Hotel already in favourites", null);

            var favourite = new Favourite
            {
                UserId = userId,
                HotelId = hotelId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _favouriteRepository.AddFavouriteAsync(favourite);

            // Load lại để trả về DTO đầy đủ
            var favourites = await _favouriteRepository.GetFavouritesByUserIdAsync(userId);
            var dto = favourites
                .Where(f => f.FavouriteId == created.FavouriteId)
                .Select(f => new FavouriteDto
                {
                    FavouriteId = f.FavouriteId,
                    HotelId = f.HotelId,
                    HotelName = f.Hotel.Name,
                    HotelLocation = f.Hotel.Location,
                    HotelImgUrl = f.Hotel.ImgUrl,
                    CityName = f.Hotel.City.Name,
                    Country = f.Hotel.City.Country,
                    MinPrice = f.Hotel.RoomTypes.Any() ? f.Hotel.RoomTypes.Min(r => r.PricePerNight) : 0,
                    MaxPrice = f.Hotel.RoomTypes.Any() ? f.Hotel.RoomTypes.Max(r => r.PricePerNight) : 0,
                    AverageRating = f.Hotel.Feedbacks.Any() ? f.Hotel.Feedbacks.Average(fb => fb.Rating) : null,
                    TotalReviews = f.Hotel.Feedbacks.Count,
                    CreatedAt = f.CreatedAt
                }).FirstOrDefault();

            return (true, "Hotel added to favourites successfully", dto);
        }

        public async Task<(bool Success, string Message)> DeleteFavouriteAsync(int favouriteId, string userId)
        {
            var deleted = await _favouriteRepository.DeleteFavouriteAsync(favouriteId, userId);

            if (!deleted)
                return (false, "Favourite not found or you don't have permission to delete it");

            return (true, "Hotel removed from favourites successfully");
        }
    }
}
