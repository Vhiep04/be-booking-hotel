using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Admin.Interfaces;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly HotelBookingContext _context;

        public ReservationRepository(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<List<Reservation>> GetByUserIdAsync(string userId, string? status, string? sort)
        {
            var query = _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room.RoomType)
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                        .ThenInclude(h => h.City)
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                        .ThenInclude(h => h.HotelImages.OrderBy(img => img.DisplayOrder))
                .Include(r => r.Payments)
                .Include(r => r.Feedbacks)
                .Where(r => r.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.PaymentStatus == status);

            query = sort switch
            {
                "oldest" => query.OrderBy(r => r.CreatedAt),
                "checkin" => query.OrderBy(r => r.CheckInDate),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            return await query.ToListAsync();
        }

        public async Task<Reservation?> GetDetailByIdAsync(int reservationId, string userId)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(room => room.RoomType)
                        .ThenInclude(rt => rt.Facilities)
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                        .ThenInclude(h => h.City)
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                        .ThenInclude(h => h.HotelImages.OrderBy(img => img.DisplayOrder))
                .Include(r => r.Payments)
                .Include(r => r.Feedbacks)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UserId == userId);
        }

        public async Task<bool> CanCancelAsync(int reservationId, string userId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UserId == userId);

            if (reservation == null) return false;

            var validStatus = reservation.PaymentStatus == "Pending"
                           || reservation.PaymentStatus == "Confirmed";
            var notCheckedIn = reservation.CheckInDate > DateOnly.FromDateTime(DateTime.Today);

            return validStatus && notCheckedIn;
        }

        public async Task<bool> CancelAsync(int reservationId, string userId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UserId == userId);

            if (reservation == null) return false;

            reservation.PaymentStatus = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}