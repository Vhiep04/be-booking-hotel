using be_booking_hotel.DTOs.Hotel;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IFavouriteService
    {
        Task<List<FavouriteDto>> GetFavouritesAsync(string userId);
        Task<(bool Success, string Message, FavouriteDto? Data)> AddFavouriteAsync(string userId, int hotelId);
        Task<(bool Success, string Message)> DeleteFavouriteAsync(int favouriteId, string userId);
    }
}
