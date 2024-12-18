using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Configuration;
using UserService.Models;
using UserService.Data;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// identity configuration
IdentityConfiguration.ConfigureServices(builder.Services, builder.Configuration);

// service config
builder.Services.AddScoped<AuthService>(); 
builder.Services.AddScoped<ClientService>(); 
builder.Services.AddScoped<AdminService>(); 

builder.Services.AddControllers();
builder.Services.AddRazorPages();  

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedData.InitializeAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configurarea middleware-urilor
app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseAuthentication(); 
app.UseAuthorization();
app.MapRazorPages(); 
app.MapControllers(); 
app.UseCors("AllowAll");

app.Run();
