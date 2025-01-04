using Microsoft.EntityFrameworkCore;
using ShoppingService.Configuration;
using ShoppingService.Data;
using ShoppingService.Models;
using ShoppingService.Service;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// app configuration
AppConfiguration.ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddDbContext<ShoppingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ShoppingServiceContextConnection")));

// Add custom services
builder.Services.AddScoped<GamesService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
AppConfiguration.ConfigureMiddleware(app);

app.Run();