using ChatService;
using Grpc.Net.Client;
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
        var channel = GrpcChannel.ForAddress("https://localhost:7223");
        var client = new Greeter.GreeterClient(channel);
        for (var item = 1; item <= 5; item++)
        {
            var adminUser = await userManager.FindByEmailAsync($"admin{item}@gmail.com");
            if (adminUser == null)
            {
                adminUser = new User()
                {
                    UserName = $"admin{item}@gmail.com",
                    Email = $"admin{item}@gmail.com",
                    FirstName = "Admin",
                    LastName = $"Number {item}",
                };
                var result = await userManager.CreateAsync(adminUser, "Parola123!");
                if (result.Succeeded)
                {
                    UserIdRequest request = new UserIdRequest()
                    {
                        Id = adminUser.Id,
                    };
                    var response = client.CreateUser(request);
                    if (response.Success == true)
                    {
                        await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
                        adminUser.EmailConfirmed = true;
                        await userManager.UpdateAsync(adminUser);
                    }
                    else
                        Console.WriteLine("Error creating user in chat service");
                }
            }
        }

        
    }
}