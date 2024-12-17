using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using UserService.Models;
using UserService.Models.DTO;

namespace UserService.Services;

public class ClientService
{
     private readonly UserManager<User> _userManager;

    public ClientService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return null;
        return new UserDto
        (
            user.Email,
            user.FirstName ?? "",
            user.LastName ?? "",
            user.PhoneNumber ?? "",
            ""
        );
    }

    public async Task<(bool Succeeded, object? Errors, User? User)> UpdateUserAsync(string id, UserDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return (false, "User not found", null);

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        return (result.Succeeded, result.Succeeded ? null : result.Errors, user);
    }
    
    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public string? GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jwtToken == null)
            return null;

        var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
        return userIdClaim?.Value;
    }
}