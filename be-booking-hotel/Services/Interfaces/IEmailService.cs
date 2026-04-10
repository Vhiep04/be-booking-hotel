using be_booking_hotel.DTOs.Payment;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string recipientName, string otpCode);
        Task SendWelcomeEmailAsync(string toEmail, string recipientName);
        Task SendPasswordResetOtpAsync(string toEmail, string recipientName, string otpCode);
        Task SendPaymentReceiptAsync(string toEmail, string recipientName, SendReceiptRequest receipt);
        Task SendCashBookingConfirmationAsync(string toEmail, string recipientName, CashReservationRequest request);
    }
}