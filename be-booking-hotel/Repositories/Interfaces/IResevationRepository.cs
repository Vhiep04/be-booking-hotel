using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IResevationRepository
    {
        Task<List<Reservation>> GetByUserIdAsync(string userId, string? status, string? sort);
        Task<Reservation?> GetDetailByIdAsync(int reservationId, string userId);
        Task<bool> CanCancelAsync(int reservationId, string userId);
        Task<bool> CancelAsync(int reservationId, string userId);
    }
}
