using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<List<Feedback>> GetFeedbacksByHotelIdAsync(int hotelId);
        Task<List<Feedback>> GetFeedbacksByUserIdAsync(string userId);
        Task<Feedback?> GetFeedbackByIdAsync(int feedbackId);
        Task<Feedback> CreateFeedbackAsync(Feedback feedback);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
        Task<bool> DeleteFeedbackAsync(int feedbackId);
        Task<bool> FeedbackExistsForReservationAsync(string userId, int reservationId);

        // Để check reservation hợp lệ
        Task<Reservation?> GetReservationByIdAsync(int reservationId);
    }
}