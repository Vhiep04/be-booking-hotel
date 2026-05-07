using be_booking_hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories.Implementations
{
    public class RoomRepository : IRoomRepository
    {
        private readonly HotelBookingContext _context;

        public RoomRepository(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(
            DateOnly? checkIn = null,
            DateOnly? checkOut = null)
        {
            var query = _context.Rooms
                .Include(r => r.RoomType)
                    .ThenInclude(rt => rt.Facilities)
                .AsQueryable();

            if (checkIn.HasValue && checkOut.HasValue)
            {
                var bookedRoomIds = await _context.Reservations
                    .Where(r => r.PaymentStatus != "Cancelled"
                        && r.CheckInDate < checkOut
                        && r.CheckOutDate > checkIn)
                    .Select(r => r.RoomId)
                    .ToListAsync();

                query = query.Where(r => !bookedRoomIds.Contains(r.RoomId));
            }

            return await query.ToListAsync();
        }
    }
}