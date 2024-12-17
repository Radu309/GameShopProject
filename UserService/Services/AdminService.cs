using Microsoft.AspNetCore.Identity;
using UserService.Models;
using UserService.Models.DTO;

namespace UserService.Services;

public class AdminService
{
    private readonly UserManager<User> _userManager;

    public AdminService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList();
        return users.Select(user => new UserDto
        (
            user.Email,
            user.FirstName ?? "",
            user.LastName ?? "",
            user.PhoneNumber ?? "",
            ""
        )).ToList();
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
}