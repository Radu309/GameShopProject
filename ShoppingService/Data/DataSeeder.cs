using Microsoft.AspNetCore.Identity;
using ShoppingService.Models;
using ShoppingService.Models.Enum;

namespace ShoppingService.Data;

public class DataSeeder
{
    public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        var roles = Enum.GetValues(typeof(Roles))
            .Cast<Roles>()
            .Select(role => role.ToString())
            .ToArray();
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
        if (adminUser == null)
        {
            adminUser = new User()
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                FirstName = "Admin",
                LastName = "Boss",
            };
            var result = await userManager.CreateAsync(adminUser, "Parola123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }
        }
    }
}