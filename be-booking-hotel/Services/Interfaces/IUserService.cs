using be_booking_hotel.DTOs.Auth;
using be_booking_hotel.DTOs.User;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, IEnumerable<string> Errors)> UpdateProfileAsync(string userId, UpdateUserDto dto);
    }
}