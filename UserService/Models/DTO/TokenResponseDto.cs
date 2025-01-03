namespace UserService.Models.DTO;

public class TokenResponseDto
{
    public string UserId { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string RedirectUrl { get; set; }
}