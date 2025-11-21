using System.ComponentModel.DataAnnotations;

namespace be_booking_hotel.DTOs.Auth
{
    public class ResendOtpDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
