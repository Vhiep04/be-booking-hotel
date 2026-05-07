using System.ComponentModel.DataAnnotations;

namespace be_booking_hotel.DTOs.User
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
    }
}