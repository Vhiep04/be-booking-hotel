using be_booking_hotel.DTOs;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<List<FeedbackDto>> GetFeedbacksByHotelAsync(int hotelId);
        Task<List<FeedbackDto>> GetMyFeedbacksAsync(string userId);
        Task<FeedbackDto?> GetFeedbackByIdAsync(int feedbackId);
        Task<(bool Success, string Message, FeedbackDto? Data)> CreateFeedbackAsync(string userId, CreateFeedbackDto dto);
        Task<(bool Success, string Message, FeedbackDto? Data)> UpdateFeedbackAsync(string userId, int feedbackId, UpdateFeedbackDto dto);
        Task<(bool Success, string Message)> DeleteFeedbackAsync(string userId, int feedbackId);
    }
}