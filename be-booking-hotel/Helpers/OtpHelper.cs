namespace be_booking_hotel.Helpers
{
    public class OtpData
    {
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public int ResendAttempts { get; set; } = 0;
        public DateTime LastResendAt { get; set; }
    }
    public class OtpHelper
    {
        public static string GenerateOtp(int length = 6)
        {
            var random = new Random();
            var otp = random.Next(0, (int)Math.Pow(10, length)).ToString($"D{length}");
            return otp;
        }

        public static string GetOtpSessionKey(string email, string purpose)
        {
            return $"OTP_{email}_{purpose}";
        }
    }
}
