namespace be_booking_hotel.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string recipientName, string otpCode);
        Task SendWelcomeEmailAsync(string toEmail, string recipientName);
        Task SendPasswordResetOtpAsync(string toEmail, string recipientName, string otpCode);
    }
}
