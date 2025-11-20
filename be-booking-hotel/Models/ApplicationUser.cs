using Microsoft.AspNetCore.Identity;

namespace be_booking_hotel.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        // Có thể thêm các thuộc tính khác
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
