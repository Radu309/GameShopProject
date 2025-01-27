using ShoppingService.Configuration;
using ShoppingService.Data;

var builder = WebApplication.CreateBuilder(args);
AppConfiguration.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
AppConfiguration.ConfigureMiddleware(app);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DataSeeder.SeedRolesAndUsers(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Eroare în timpul seeding-ului: {ex.Message}");
    }
}

app.Run();