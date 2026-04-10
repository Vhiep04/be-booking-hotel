using be_booking_hotel.DTOs.Payment;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<int> CreateReservationAsync(SendReceiptRequest request);
        Task CreatePaymentAsync(int reservationId, SendReceiptRequest request);
        Task<int> CreateCashReservationAsync(CashReservationRequest request);
        Task UpdateReservationStatusAsync(int reservationId, string status);
    }
}
