using be_booking_hotel.Helpers;
using System.Text.Json;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services.Implements
{
    public class OtpService: IOtpService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<OtpService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateOtp()
        {
            var otpLength = int.Parse(_configuration["OtpSettings:Length"] ?? "6");
            return OtpHelper.GenerateOtp(otpLength);
        }

        public void SaveOtpToSession(string email, string userId, string otpCode, string purpose)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                _logger.LogError("Session is not available");
                throw new InvalidOperationException("Session is not available");
            }

            var expirationMinutes = int.Parse(_configuration["OtpSettings:ExpirationMinutes"] ?? "5");

            var otpData = new OtpData
            {
                OtpCode = otpCode,
                Email = email,
                UserId = userId,
                Purpose = purpose,
                ExpiresAt = DateTime.Now.AddMinutes(expirationMinutes),
                ResendAttempts = 0,
                LastResendAt = DateTime.Now
            };

            var sessionKey = OtpHelper.GetOtpSessionKey(email, purpose);
            session.SetString(sessionKey, JsonSerializer.Serialize(otpData));

            _logger.LogInformation($"OTP saved to session for {email}, purpose: {purpose}");
        }

        public bool ValidateOtp(string email, string otpCode, string purpose)
        {
            var otpData = GetOtpData(email, purpose);

            if (otpData == null)
            {
                _logger.LogWarning($"OTP not found for {email}, purpose: {purpose}");
                return false;
            }

            if (otpData.ExpiresAt < DateTime.Now)
            {
                _logger.LogWarning($"OTP expired for {email}");
                InvalidateOtp(email, purpose);
                return false;
            }

            if (otpData.OtpCode != otpCode)
            {
                _logger.LogWarning($"Invalid OTP code for {email}");
                return false;
            }

            // OTP hợp lệ, xóa khỏi session
            InvalidateOtp(email, purpose);
            _logger.LogInformation($"OTP validated successfully for {email}");
            return true;
        }

        public OtpData? GetOtpData(string email, string purpose)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var sessionKey = OtpHelper.GetOtpSessionKey(email, purpose);
            var otpJson = session.GetString(sessionKey);

            if (string.IsNullOrEmpty(otpJson)) return null;

            return JsonSerializer.Deserialize<OtpData>(otpJson);
        }

        public void InvalidateOtp(string email, string purpose)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var sessionKey = OtpHelper.GetOtpSessionKey(email, purpose);
            session.Remove(sessionKey);

            _logger.LogInformation($"OTP invalidated for {email}, purpose: {purpose}");
        }

        public bool CanResendOtp(string email, string purpose)
        {
            var otpData = GetOtpData(email, purpose);
            if (otpData == null) return true;

            var maxAttempts = int.Parse(_configuration["OtpSettings:MaxResendAttempts"] ?? "3");
            var cooldownMinutes = int.Parse(_configuration["OtpSettings:ResendCooldownMinutes"] ?? "1");

            // Check số lần resend
            if (otpData.ResendAttempts >= maxAttempts)
            {
                _logger.LogWarning($"Max resend attempts reached for {email}");
                return false;
            }

            // Check cooldown time
            if ((DateTime.Now - otpData.LastResendAt).TotalMinutes < cooldownMinutes)
            {
                _logger.LogWarning($"Resend cooldown active for {email}");
                return false;
            }

            return true;
        }

        public void IncrementResendAttempt(string email, string purpose)
        {
            var otpData = GetOtpData(email, purpose);
            if (otpData == null) return;

            otpData.ResendAttempts++;
            otpData.LastResendAt = DateTime.Now;

            var session = _httpContextAccessor.HttpContext?.Session;
            var sessionKey = OtpHelper.GetOtpSessionKey(email, purpose);
            session?.SetString(sessionKey, JsonSerializer.Serialize(otpData));

            _logger.LogInformation($"Resend attempt incremented for {email}: {otpData.ResendAttempts}");
        }
    }
}
