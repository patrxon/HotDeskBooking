using Microsoft.AspNetCore.Identity;
using HotDeskBooking.Models;

namespace HotDeskBooking.Helpers
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedAdminAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {

            var administrator = new User { Name = "Admin", UserName = "Admin", Email = "Admin@Admin" };
            var result = await userManager.CreateAsync(administrator, "Admin1!");

            if (result.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                await userManager.AddToRoleAsync(administrator, "Admin");
            }
        }
    }
}
