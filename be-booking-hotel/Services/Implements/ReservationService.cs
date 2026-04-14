using be_booking_hotel.DTOs.Resevation;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository reservationRepository,
            INotificationService notificationService,
            ILogger<ReservationService> logger)
        {
            _reservationRepository = reservationRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<ReservationDto>> GetMyReservationsAsync(
            string userId, string? status, string? sort)
        {
            var reservations = await _reservationRepository.GetByUserIdAsync(userId, status, sort);
            return reservations.Select(MapToDto).ToList();
        }

        public async Task<ReservationDto?> GetMyReservationDetailAsync(
            int reservationId, string userId)
        {
            var reservation = await _reservationRepository.GetDetailByIdAsync(reservationId, userId);
            return reservation == null ? null : MapToDto(reservation);
        }

        public async Task<(bool Success, string Message)> CancelReservationAsync(
            int reservationId, string userId)
        {
            if (!await _reservationRepository.CanCancelAsync(reservationId, userId))
                return (false, "Cannot cancel this reservation.");

            var success = await _reservationRepository.CancelAsync(reservationId, userId);
            if (!success)
                return (false, "Reservation not found.");

            // Gửi notification
            var reservation = await _reservationRepository.GetDetailByIdAsync(reservationId, userId);
            if (reservation != null)
            {
                try
                {
                    await _notificationService.NotifyBookingCancelled(reservation, cancelledByUser: true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Notification failed for reservation {Id}", reservationId);
                }
            }

            return (true, "Reservation cancelled successfully.");
        }

        private static ReservationDto MapToDto(Reservation r)
        {
            var nights = r.CheckOutDate.DayNumber - r.CheckInDate.DayNumber;
            var payment = r.Payments?.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
            var feedback = r.Feedbacks?.FirstOrDefault();

            return new ReservationDto
            {
                ReservationId = r.ReservationId,
                BookingCode = $"BK-{r.CreatedAt.Year}-{r.ReservationId:D3}",

                RoomId = r.RoomId,
                RoomNumber = r.Room?.RoomNumber ?? "",
                RoomTypeName = r.Room?.RoomType?.TypeName ?? "",
                PricePerNight = r.Room?.RoomType?.PricePerNight ?? 0,
                Capacity = r.Room?.RoomType?.Capacity ?? 0,
                HotelId = r.Room?.HotelId ?? 0,
                HotelName = r.Room?.Hotel?.Name ?? "",
                HotelLocation = r.Room?.Hotel?.Location ?? "",
                HotelImage = r.Room?.Hotel?.HotelImages?
                                   .FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl
                                 ?? r.Room?.Hotel?.ImgUrl,
                CityName = r.Room?.Hotel?.City?.Name ?? "",

                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                Nights = nights,
                TotalPrice = r.TotalPrice,
                PaymentStatus = r.PaymentStatus ?? "Pending",
                CreatedAt = r.CreatedAt,

                PaymentMethod = payment?.PaymentMethod,
                PaymentDate = payment?.PaymentDate,

                HasFeedback = feedback != null,
                FeedbackId = feedback?.FeedbackId,
                Rating = feedback?.Rating,
                Comment = feedback?.Comment
            };
        }
    }
}