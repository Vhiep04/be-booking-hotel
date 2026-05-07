using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories.Implementations
{
    public class FavouriteRepository : IFavouriteRepository
    {
        private readonly HotelBookingContext _context;

        public FavouriteRepository(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<List<Favourite>> GetFavouritesByUserIdAsync(string userId)
        {
            return await _context.Favourites
                .Include(f => f.Hotel)
                    .ThenInclude(h => h.City)
                .Include(f => f.Hotel)
                    .ThenInclude(h => h.RoomTypes)
                .Include(f => f.Hotel)
                    .ThenInclude(h => h.Feedbacks)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Favourite?> GetFavouriteAsync(string userId, int hotelId)
        {
            return await _context.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.HotelId == hotelId);
        }

        public async Task<Favourite> AddFavouriteAsync(Favourite favourite)
        {
            _context.Favourites.Add(favourite);
            await _context.SaveChangesAsync();
            return favourite;
        }

        public async Task<bool> DeleteFavouriteAsync(int favouriteId, string userId)
        {
            var favourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.FavouriteId == favouriteId && f.UserId == userId);

            if (favourite == null) return false;

            _context.Favourites.Remove(favourite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFavouriteExistsAsync(string userId, int hotelId)
        {
            return await _context.Favourites
                .AnyAsync(f => f.UserId == userId && f.HotelId == hotelId);
        }
    }
}
