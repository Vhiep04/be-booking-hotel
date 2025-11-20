using Microsoft.AspNetCore.Identity;

namespace be_booking_hotel.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "User", "Manager" };

            foreach (var roleName in roleNames)
            {
                // Kiểm tra role đã tồn tại chưa
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Tạo role mới
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}