using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTO;

public class RegisterDto
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string ConfirmPassword { get; set; } 
}