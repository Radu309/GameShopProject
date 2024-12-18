using Microsoft.EntityFrameworkCore;
using ShoppingService.Data;
using ShoppingService.Models;
using ShoppingService.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShoppingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ShoppingServiceContextConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); 
builder.Services.AddScoped<GamesService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// de realizat mai tarziu
// builder.Services.AddAuthorization(opts => {
//     opts.AddPolicy("Admin", policy => {
//         policy.RequireClaim("Admin");
//     });
//     opts.AddPolicy("Customer", policy => {
//         policy.RequireClaim("Customer");
//     });
// });
// builder.Services.ConfigureApplicationCookie(options =>
// {
//     options.LoginPath = "/Identity/Account/Login";
//     options.AccessDeniedPath = "/Identity/Account/AccessDenied";
//     options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Expirare sesiune
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.MapControllers();
app.MapRazorPages();

app.UseAuthorization();
app.UseAuthentication();

app.UseRouting();
app.UseHttpsRedirection();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Games}/{action=Index}/{id?}"
);
app.UseCors("AllowAll");

app.Run();