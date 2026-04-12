using be_booking_hotel.DTOs.Payment;
using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<int> CreateReservationAsync(SendReceiptRequest request);
        Task<Payment> CreatePaymentAsync(int reservationId, SendReceiptRequest request); // ✅ đổi void → Payment
        Task<Reservation?> GetReservationByIdAsync(int reservationId);                  // ✅ thêm mới
        Task UpdateReservationStatusAsync(int reservationId, string status);
        Task<int> CreateCashReservationAsync(CashReservationRequest request);
    }
}
