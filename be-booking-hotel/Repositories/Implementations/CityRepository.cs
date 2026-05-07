using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories
{
    
    /// Repository cho City - Sử dụng LINQ với EF Core (DB First)
    /// Fixed cho many-to-many relationship (Room <-> Facility)
    
    public class CityRepository : ICityRepository
    {
        private readonly HotelBookingContext _context;

        public CityRepository(HotelBookingContext context)
        {
            _context = context;
        }

        
        /// Lấy tất cả cities kèm images
        
        public async Task<List<City>> GetAllAsync()
        {
            return await _context.Cities
                .Include(c => c.CityImages.OrderBy(img => img.DisplayOrder))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        
        /// Lấy city theo ID
        
        public async Task<City?> GetByIdAsync(int id)
        {
            return await _context.Cities
                .Include(c => c.CityImages.OrderBy(img => img.DisplayOrder))
                .FirstOrDefaultAsync(c => c.CityId == id);
        }

        
        /// Search cities theo tên hoặc quốc gia
        /// FIXED: Handle nullable IsPrimary
        
        public async Task<List<City>> SearchByNameOrCountryAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.Cities
                .Include(c => c.CityImages.Where(img => img.IsPrimary == true))
                .Where(c => c.Name.ToLower().Contains(lowerSearchTerm) ||
                           c.Country.ToLower().Contains(lowerSearchTerm))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        
        /// Lấy tất cả hotels trong city kèm rooms và facilities
        /// FIXED: Dùng navigation property trực tiếp thay vì RoomFacilities
        
        public async Task<List<Hotel>> GetHotelsByCityIdAsync(int cityId)
        {
            return await _context.Hotels
                .Include(h => h.HotelImages.OrderBy(img => img.DisplayOrder))
                .Include(h => h.RoomTypes)
                .ThenInclude(r => r.Facilities)
                .Include(h => h.RoomTypes)
                .ThenInclude(rt => rt.Rooms)
                .ThenInclude(room => room.Reservations)
                .Include(h => h.Feedbacks)
                .Where(h => h.CityId == cityId)
                .ToListAsync();
        }


        /// Lấy một hotel cụ thể trong city

        public async Task<Hotel?> GetHotelInCityAsync(int cityId, int hotelId)
        {
            return await _context.Hotels
                .Include(h => h.City)
                .Include(h => h.HotelImages.OrderBy(img => img.DisplayOrder))
                .Include(h => h.RoomTypes)
                    .ThenInclude(r => r.Facilities)
                .Include(h => h.Feedbacks)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId && h.CityId == cityId);
        }

        
        /// Kiểm tra city có tồn tại không
        
        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.CityId == cityId);
        }

        
        /// Lấy rooms của hotel kèm facilities
        
        public async Task<List<Room>> GetRoomsByHotelIdAsync(int hotelId)
        {
            return await _context.Rooms
                .Include(r => r.RoomType.Facilities) // ← Direct navigation
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
        }

        
        /// Lấy facilities của một room
        /// FIXED: Dùng navigation property của Room
        
        public async Task<List<Facility>> GetRoomFacilitiesAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType.Facilities) // ← Direct navigation
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            return room?.RoomType.Facilities?.ToList() ?? new List<Facility>();
        }

        
        /// Lấy images của city
        
        public async Task<List<CityImage>> GetCityImagesAsync(int cityId)
        {
            return await _context.CityImages
                .Where(img => img.CityId == cityId)
                .OrderBy(img => img.DisplayOrder)
                .ToListAsync();
        }

        /// Lấy images của hotel
        
        public async Task<List<HotelImage>> GetHotelImagesAsync(int hotelId)
        {
            return await _context.HotelImages
                .Where(img => img.HotelId == hotelId)
                .OrderBy(img => img.DisplayOrder)
                .ToListAsync();
        }


        /// Lấy TẤT CẢ hotels từ mọi city

        public async Task<List<Hotel>> GetAllHotelsAsync()
        {
            return await _context.Hotels
                .Include(h => h.City)
                .Include(h => h.HotelImages.OrderBy(img => img.DisplayOrder))
                .Include(h => h.RoomTypes)
                .ThenInclude(r => r.Facilities)
                .Include(h => h.RoomTypes)
                .ThenInclude(rt => rt.Rooms)
                .ThenInclude(room => room.Reservations)
                .Include(h => h.Feedbacks)
                .OrderBy(h => h.City.Name)
                .ThenBy(h => h.Name)
                .ToListAsync();
        }

        /// Lấy tất cả RoomType (id + name)

        public async Task<List<RoomType>> GetAllRoomTypesAsync()
        {
            return await _context.RoomTypes
                .OrderBy(r => r.TypeName)
                .ToListAsync();
        }

        
        /// Lấy danh sách TypeName unique từ tất cả RoomTypes
        
        public async Task<List<string>> GetDistinctRoomTypeNamesAsync()
        {
            return await _context.RoomTypes
                .Select(r => r.TypeName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }
    }
}