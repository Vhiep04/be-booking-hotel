using be_booking_hotel.DTOs.Auth;
using be_booking_hotel.DTOs.User;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<(bool Success, IEnumerable<string> Errors)> UpdateProfileAsync(
            string userId, UpdateUserDto dto)
            => _userRepository.UpdateProfileAsync(userId, dto);
    }
}