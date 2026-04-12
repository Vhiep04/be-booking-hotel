using be_booking_hotel.DTOs.Resevation;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IReservationService
    {
        Task<List<ReservationDto>> GetMyReservationsAsync(
            string userId, string? status, string? sort);

        Task<ReservationDto?> GetMyReservationDetailAsync(
            int reservationId, string userId);

        Task<(bool Success, string Message)> CancelReservationAsync(
            int reservationId, string userId);
    }
}
