using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories
{
    /// <summary>
    /// Repository cho City - Sử dụng LINQ với EF Core (DB First)
    /// Fixed cho many-to-many relationship (Room <-> Facility)
    /// </summary>
    public class CityRepository : ICityRepository
    {
        private readonly HotelBookingContext _context;

        public CityRepository(HotelBookingContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả cities kèm images
        /// </summary>
        public async Task<List<City>> GetAllAsync()
        {
            return await _context.Cities
                .Include(c => c.CityImages.OrderBy(img => img.DisplayOrder))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy city theo ID
        /// </summary>
        public async Task<City?> GetByIdAsync(int id)
        {
            return await _context.Cities
                .Include(c => c.CityImages.OrderBy(img => img.DisplayOrder))
                .FirstOrDefaultAsync(c => c.CityId == id);
        }

        /// <summary>
        /// Search cities theo tên hoặc quốc gia
        /// FIXED: Handle nullable IsPrimary
        /// </summary>
        public async Task<List<City>> SearchByNameOrCountryAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.Cities
                .Include(c => c.CityImages.Where(img => img.IsPrimary == true)) // FIXED: Compare with true
                .Where(c => c.Name.ToLower().Contains(lowerSearchTerm) ||
                           c.Country.ToLower().Contains(lowerSearchTerm))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả hotels trong city kèm rooms và facilities
        /// FIXED: Dùng navigation property trực tiếp thay vì RoomFacilities
        /// </summary>
        public async Task<List<Hotel>> GetHotelsByCityIdAsync(int cityId)
        {
            return await _context.Hotels
                .Include(h => h.HotelImages.OrderBy(img => img.DisplayOrder))
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Facilities) // ← Direct navigation (many-to-many)
                .Include(h => h.Feedbacks)
                .Where(h => h.CityId == cityId)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy một hotel cụ thể trong city
        /// </summary>
        public async Task<Hotel?> GetHotelInCityAsync(int cityId, int hotelId)
        {
            return await _context.Hotels
                .Include(h => h.City)
                .Include(h => h.HotelImages.OrderBy(img => img.DisplayOrder))
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Facilities) // ← Direct navigation
                .Include(h => h.Feedbacks)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId && h.CityId == cityId);
        }

        /// <summary>
        /// Kiểm tra city có tồn tại không
        /// </summary>
        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.CityId == cityId);
        }

        /// <summary>
        /// Lấy rooms của hotel kèm facilities
        /// </summary>
        public async Task<List<Room>> GetRoomsByHotelIdAsync(int hotelId)
        {
            return await _context.Rooms
                .Include(r => r.Facilities) // ← Direct navigation
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy facilities của một room
        /// FIXED: Dùng navigation property của Room
        /// </summary>
        public async Task<List<Facility>> GetRoomFacilitiesAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Facilities) // ← Direct navigation
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            return room?.Facilities?.ToList() ?? new List<Facility>();
        }

        /// <summary>
        /// Lấy images của city
        /// </summary>
        public async Task<List<CityImage>> GetCityImagesAsync(int cityId)
        {
            return await _context.CityImages
                .Where(img => img.CityId == cityId)
                .OrderBy(img => img.DisplayOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy images của hotel
        /// </summary>
        public async Task<List<HotelImage>> GetHotelImagesAsync(int hotelId)
        {
            return await _context.HotelImages
                .Where(img => img.HotelId == hotelId)
                .OrderBy(img => img.DisplayOrder)
                .ToListAsync();
        }
    }
}