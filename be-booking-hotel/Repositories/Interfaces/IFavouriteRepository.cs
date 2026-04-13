using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IFavouriteRepository
    {
        Task<List<Favourite>> GetFavouritesByUserIdAsync(string userId);
        Task<Favourite?> GetFavouriteAsync(string userId, int hotelId);
        Task<Favourite> AddFavouriteAsync(Favourite favourite);
        Task<bool> DeleteFavouriteAsync(int favouriteId, string userId);
        Task<bool> IsFavouriteExistsAsync(string userId, int hotelId);
    }
}
