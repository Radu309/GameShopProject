using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTO;
using UserService.Services;

namespace UserService.Controllers;

[Route("api/users/")]
[ApiController]
[Authorize(Roles = "Admin, Client")]
public class ClientController : ControllerBase
{
    private readonly ClientService _clientService;

    public ClientController(ClientService clientService)
    {
        _clientService = clientService;
    }

    private async Task<(string? UserId, IActionResult? ErrorResult)> GetUserIdFromTokenAsync()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return (null, BadRequest("Token is required"));

        var userId = _clientService.GetUserIdFromToken(token);
        if (userId == null)
            return (null, Unauthorized("Invalid token"));

        return (userId, null);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUser()
    {
        var (userId, errorResult) = await GetUserIdFromTokenAsync();
        if (errorResult != null)
            return errorResult;

        var userDto = await _clientService.GetUserByIdAsync(userId!);
        if (userDto == null)
            return NotFound("User not found");

        return Ok(userDto);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UserDto model)
    {
        var (userId, errorResult) = await GetUserIdFromTokenAsync();
        if (errorResult != null)
            return errorResult;

        var result = await _clientService.UpdateUserAsync(userId!, model);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(result.User);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUser()
    {
        var (userId, errorResult) = await GetUserIdFromTokenAsync();
        if (errorResult != null)
            return errorResult;

        var success = await _clientService.DeleteUserAsync(userId!);
        if (!success)
            return BadRequest("Failed to delete user");

        return Ok("User deleted");
    }
}

