using System.ComponentModel.DataAnnotations;

namespace ChatService.Model;

public class Message
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; }
    public bool DeleteForYou { get; set; } = false;
    public bool DeleteForEveryone { get; set; } = false;
    public bool IsRead { get; set; } = false;
    
    public string ChatId { get; set; }
    public Chat Chat { get; set; }
}