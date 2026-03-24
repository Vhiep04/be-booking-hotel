using be_booking_hotel.DTOs.Hotel;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories.Implementations
{
    public class HotelRepository : IHotelRepository
    {
        private readonly HotelBookingContext _context;
        private readonly ILogger<HotelRepository> _logger;

        public HotelRepository(
            HotelBookingContext context,
            ILogger<HotelRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<Hotel> Hotels, int TotalCount)> GetHotelsAsync(HotelFilterDto filter)
        {
            var query = _context.Hotels
                .Include(h => h.City)
                .Include(h => h.Rooms)
                .Include(h => h.Feedbacks)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(h =>
                    h.Name.Contains(filter.Search) ||
                    h.Location.Contains(filter.Search) ||
                    h.City.Name.Contains(filter.Search)
                );
            }

            // City filter
            if (filter.CityId.HasValue)
            {
                query = query.Where(h => h.CityId == filter.CityId.Value);
            }

            // Country filter
            if (!string.IsNullOrWhiteSpace(filter.Country))
            {
                query = query.Where(h => h.City.Country.Contains(filter.Country));
            }

            // Price filter
            if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
            {
                query = query.Where(h =>
                    h.Rooms.Any(r =>
                        (!filter.MinPrice.HasValue || r.PricePerNight >= filter.MinPrice.Value) &&
                        (!filter.MaxPrice.HasValue || r.PricePerNight <= filter.MaxPrice.Value)
                    )
                );
            }

            // Rating filter
            if (filter.MinRating.HasValue)
            {
                query = query.Where(h =>
                    h.Feedbacks.Any() &&
                    h.Feedbacks.Average(f => f.Rating) >= filter.MinRating.Value
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(h => h.Rooms.Min(r => r.PricePerNight)),
                "price_desc" => query.OrderByDescending(h => h.Rooms.Min(r => r.PricePerNight)),
                "rating" => query.OrderByDescending(h => h.Feedbacks.Any() ? h.Feedbacks.Average(f => f.Rating) : 0),
                "name" => query.OrderBy(h => h.Name),
                _ => query.OrderByDescending(h => h.CreatedAt) // Default: newest first
            };

            // Pagination
            var hotels = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (hotels, totalCount);
        }

        public async Task<Hotel?> GetHotelByIdAsync(int hotelId)
        {
            return await _context.Hotels
                .Include(h => h.City)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Facilities)
                .Include(h => h.Feedbacks)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);
        }

        public async Task<List<Room>> GetHotelRoomsAsync(int hotelId)
        {
            return await _context.Rooms
                .Include(r => r.Facilities)
                .Where(r => r.HotelId == hotelId)
                .OrderBy(r => r.PricePerNight)
                .ToListAsync();
        }

        public async Task<bool> HotelExistsAsync(int hotelId)
        {
            return await _context.Hotels.AnyAsync(h => h.HotelId == hotelId);
        }

        public async Task<List<Feedback>> GetHotelFeedbacksAsync(int hotelId, int limit = 5)
        {
            return await _context.Feedbacks
                .Where(f => f.HotelId == hotelId)
                .OrderByDescending(f => f.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int hotelId)
        {
            var feedbacks = await _context.Feedbacks
                .Where(f => f.HotelId == hotelId)
                .GroupBy(f => f.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var distribution = new Dictionary<int, int>
            {
                { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
            };

            foreach (var item in feedbacks)
            {
                distribution[item.Rating] = item.Count;
            }

            return distribution;
        }

        public async Task<Room?> GetRoomByIdAsync(int hotelId, int roomId)
        {
            return await _context.Rooms
                .Include(r => r.Facilities)
                .FirstOrDefaultAsync(r => r.HotelId == hotelId && r.RoomId == roomId);
        }
    }
}