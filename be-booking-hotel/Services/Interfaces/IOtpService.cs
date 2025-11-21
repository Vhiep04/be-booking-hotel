using be_booking_hotel.Helpers;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp();
        void SaveOtpToSession(string email, string userId, string otpCode, string purpose);
        bool ValidateOtp(string email, string otpCode, string purpose);
        OtpData? GetOtpData(string email, string purpose);
        void InvalidateOtp(string email, string purpose);
        bool CanResendOtp(string email, string purpose);
        void IncrementResendAttempt(string email, string purpose);
    }
}
