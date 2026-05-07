using be_booking_hotel.DTOs.Auth;
using be_booking_hotel.DTOs.User;
using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string userId);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateProfileAsync(string userId, UpdateUserDto dto);
    }
}