namespace be_booking_hotel.DTOs.Auth
{
    public class GoogleLoginDto
    {
        public string IdToken { get; set; } = string.Empty;
    }

    // DTO
    public class GoogleCodeDto
    {
        public string Code { get; set; } = string.Empty;
    }
}