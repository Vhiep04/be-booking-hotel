using be_booking_hotel.DTOs.User;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace be_booking_hotel.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateProfileAsync(
            string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found." });

            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.BirthDate != null) user.BirthDate = dto.BirthDate;
            if (dto.Address != null) user.Address = dto.Address;
            if (dto.Gender != null) user.Gender = dto.Gender;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            return (true, Enumerable.Empty<string>());
        }
    }
}
