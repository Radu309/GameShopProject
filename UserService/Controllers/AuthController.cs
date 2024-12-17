using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Models.DTO;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/auth/")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); 
        }

        var result = await _authService.RegisterAsync(model);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }
        return Ok(new { Message = "User created successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        var user = await _authService.AuthenticateAsync(model);
        if (user == null)
            return Unauthorized("Invalid login attempt");
        
        var refreshToken = await _authService.GenerateJwtToken(user);
        await _authService.SaveRefreshTokenAsync(user.Id, refreshToken);
        
        var response = new TokenResponseDto()
        {
            UserId = user.Id,
            RefreshToken = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddMinutes(5)
        };

        return Ok(response);
    }
    [Authorize(Roles = "Admin, Client")]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto refreshTokenDto)
    {
        var user = await _authService.GetUserFromToken(refreshTokenDto.RefreshToken);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }
        if (user.Id == null || !await _authService.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
        {
            return Unauthorized("Invalid refresh token.");
        }
        //nu mai e nevoie de invalidarea tokenului. Acesta se suprascrie
        var newRefreshToken = await _authService.GenerateJwtToken(user);
        await _authService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

        // var response = new TokenModel()
        var response = new TokenResponseDto()
        {
            UserId = user.Id,
            RefreshToken = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddMinutes(5)
        };

        return Ok(response);
    }
    [Authorize(Roles = "Admin, Client")]
    [HttpGet("check-token")]
    public async Task<IActionResult> GetBearerToken()
    {
        var authHeader = Request.Headers["Authorization"].ToString();   
        if(!authHeader.StartsWith("Bearer "))
            return BadRequest();
        if (string.IsNullOrEmpty(authHeader))
            return Unauthorized("Bearer token is missing");

        var bearerToken = authHeader.Substring("Bearer ".Length).Trim();
        if (!await _authService.ValidateRefreshTokenAsync(bearerToken))
            return Unauthorized("Bearer token is invalid.");

        return Ok(new { BearerToken = bearerToken });
    }
}