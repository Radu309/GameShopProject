using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ShoppingService.Data;
using ShoppingService.Models;
using ShoppingService.Service;

namespace ShoppingService.Configuration;

public static class AppConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<GamesService>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddDbContext<ShoppingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ShoppingServiceContextConnection")));

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddControllersWithViews();
        services.AddRazorPages();
        
        ConfigureIdentity(services);
    }
    
    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ShoppingDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
        });
        
        services.AddAuthorization(options => {
            options.AddPolicy("Admin", policy => {
                policy.RequireClaim("Admin");
            });
            options.AddPolicy("Customer", policy => {
                policy.RequireClaim("Customer");
            });
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30); 
        });
    }
    
    public static void ConfigureMiddleware(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Games}/{action=Index}/{id?}");

        app.MapRazorPages();
        
    }
    
}