using ShoppingService.Configuration;
using Microsoft.EntityFrameworkCore;
using ShoppingService.Data;
using ShoppingService.Hubs;
using ShoppingService.Models;

var builder = WebApplication.CreateBuilder(args);


AppConfiguration.ConfigureServices(builder.Services, builder.Configuration);
//provizorii
builder.Services.AddSignalR();
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
//provizorii
app.UseRouting();
app.MapHub<ChatHub>("/chatHub");

app.Run();