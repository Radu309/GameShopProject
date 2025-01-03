using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Data;
using UserService.Models;
using UserService.Models.DTO;

namespace UserService.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IdentityDbContext _context;

    public AuthService(UserManager<User> userManager, IConfiguration configuration, IdentityDbContext context)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterRequestDto model)
    {
        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        await _userManager.AddToRoleAsync(user, "Client"); 

        return result;
    }

    public async Task<User?> AuthenticateAsync(LoginRequestDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            return user;
        return null;
    }

    public async Task<string> GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, _userManager.GetRolesAsync(user).Result.First()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("Invalid userId");
        }

        await _userManager.SetAuthenticationTokenAsync(
            user,
            "CustomProvider",
            "RefreshToken",
            refreshToken
        );
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        var user = await GetUserFromToken(refreshToken);
        if (user == null)
            return false;

        var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "CustomProvider", "RefreshToken");
        return storedToken == refreshToken;
    }

    public async Task<User> GetUserFromToken(string token)
    {
        var tokenUser = await _context.UserTokens
            .FirstOrDefaultAsync(t => t.Value == token);
        if (tokenUser == null)
            return null;
        return await _userManager.FindByIdAsync(tokenUser.UserId);
    }
    
}