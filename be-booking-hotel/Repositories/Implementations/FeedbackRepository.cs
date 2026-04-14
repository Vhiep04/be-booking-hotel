using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories.Implementations
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly HotelBookingContext _context;

        public FeedbackRepository(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<List<Feedback>> GetFeedbacksByHotelIdAsync(int hotelId)
        {
            return await _context.Feedbacks
                .Include(f => f.Hotel)
                .Where(f => f.HotelId == hotelId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbacksByUserIdAsync(string userId)
        {
            return await _context.Feedbacks
                .Include(f => f.Hotel)
                .Include(f => f.Reservation)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int feedbackId)
        {
            return await _context.Feedbacks
                .Include(f => f.Hotel)
                .Include(f => f.Reservation)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
        }

        public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
        {
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return false;

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FeedbackExistsForReservationAsync(string userId, int reservationId)
        {
            return await _context.Feedbacks
                .AnyAsync(f => f.UserId == userId && f.ReservationId == reservationId);
        }

        public async Task<Reservation?> GetReservationByIdAsync(int reservationId)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        }
    }
}