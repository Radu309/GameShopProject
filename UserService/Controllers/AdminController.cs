using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTO;
using UserService.Services;

namespace UserService.Controllers;

[Route("api/admin/users/")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService )
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        if (users == null || !users.Any())
            return NotFound("No users found");
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var userDto = await _adminService.GetUserByIdAsync(id);
        if (userDto == null)
            return NotFound("User not found");
        return Ok(userDto);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUser([FromBody] UserDto model, string id)
    {
        var result = await _adminService.UpdateUserAsync(id, model);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return Ok(result.User);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await _adminService.DeleteUserAsync(id);
        if (!success)
            return BadRequest("Failed to delete user");
        return Ok("User deleted");
    }
}
