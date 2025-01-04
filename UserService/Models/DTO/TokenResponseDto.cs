namespace UserService.Models.DTO;

public class TokenResponseDto
{
    public string RefreshToken { get; set; }
    public string RedirectUrl { get; set; }
}