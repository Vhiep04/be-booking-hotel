using be_booking_hotel.DTOs;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services.Implements
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(
            IFeedbackRepository feedbackRepository,
            ILogger<FeedbackService> logger)
        {
            _feedbackRepository = feedbackRepository;
            _logger = logger;
        }

        public async Task<List<FeedbackDto>> GetFeedbacksByHotelAsync(int hotelId)
        {
            var feedbacks = await _feedbackRepository.GetFeedbacksByHotelIdAsync(hotelId);
            return feedbacks.Select(MapToDto).ToList();
        }

        public async Task<List<FeedbackDto>> GetMyFeedbacksAsync(string userId)
        {
            var feedbacks = await _feedbackRepository.GetFeedbacksByUserIdAsync(userId);
            return feedbacks.Select(MapToDto).ToList();
        }

        public async Task<FeedbackDto?> GetFeedbackByIdAsync(int feedbackId)
        {
            var feedback = await _feedbackRepository.GetFeedbackByIdAsync(feedbackId);
            return feedback == null ? null : MapToDto(feedback);
        }

        public async Task<(bool Success, string Message, FeedbackDto? Data)> CreateFeedbackAsync(
            string userId, CreateFeedbackDto dto)
        {
            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
                return (false, "Rating must be between 1 and 5", null);

            // Check reservation tồn tại và thuộc về user
            var reservation = await _feedbackRepository.GetReservationByIdAsync(dto.ReservationId);

            if (reservation == null)
                return (false, $"Reservation with ID {dto.ReservationId} not found", null);

            if (reservation.UserId != userId)
                return (false, "You don't have permission to review this reservation", null);

            if (reservation.PaymentStatus?.ToLower() != "completed")
                return (false, "You can only leave a review after payment is completed", null);

            if (reservation.Room.HotelId != dto.HotelId)
                return (false, "Hotel does not match the reservation", null);

            var alreadyReviewed = await _feedbackRepository
                .FeedbackExistsForReservationAsync(userId, dto.ReservationId);

            if (alreadyReviewed)
                return (false, "You have already reviewed this reservation", null);

            var feedback = new Feedback
            {
                UserId = userId,
                HotelId = dto.HotelId,
                ReservationId = dto.ReservationId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _feedbackRepository.CreateFeedbackAsync(feedback);

            // Load lại để có navigation properties
            var result = await _feedbackRepository.GetFeedbackByIdAsync(created.FeedbackId);
            return (true, "Feedback created successfully", MapToDto(result!));
        }

        public async Task<(bool Success, string Message, FeedbackDto? Data)> UpdateFeedbackAsync(
            string userId, int feedbackId, UpdateFeedbackDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return (false, "Rating must be between 1 and 5", null);

            var feedback = await _feedbackRepository.GetFeedbackByIdAsync(feedbackId);

            if (feedback == null)
                return (false, $"Feedback with ID {feedbackId} not found", null);

            // Chỉ owner mới được sửa
            if (feedback.UserId != userId)
                return (false, "You don't have permission to update this feedback", null);

            feedback.Rating = dto.Rating;
            feedback.Comment = dto.Comment;

            var updated = await _feedbackRepository.UpdateFeedbackAsync(feedback);
            var result = await _feedbackRepository.GetFeedbackByIdAsync(updated.FeedbackId);
            return (true, "Feedback updated successfully", MapToDto(result!));
        }

        public async Task<(bool Success, string Message)> DeleteFeedbackAsync(string userId, int feedbackId)
        {
            var feedback = await _feedbackRepository.GetFeedbackByIdAsync(feedbackId);

            if (feedback == null)
                return (false, $"Feedback with ID {feedbackId} not found");

            // Chỉ owner mới được xóa
            if (feedback.UserId != userId)
                return (false, "You don't have permission to delete this feedback");

            await _feedbackRepository.DeleteFeedbackAsync(feedbackId);
            return (true, "Feedback deleted successfully");
        }

        // ── Helper ──────────────────────────────────────────
        private static FeedbackDto MapToDto(Feedback f) => new()
        {
            FeedbackId = f.FeedbackId,
            UserId = f.UserId,
            UserName = "Anonymous",          // TODO: join AspNetUsers nếu cần
            HotelId = f.HotelId,
            HotelName = f.Hotel?.Name ?? "",
            ReservationId = f.ReservationId,
            Rating = f.Rating,
            Comment = f.Comment,
            CreatedAt = f.CreatedAt
        };
    }
}