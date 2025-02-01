using System.ComponentModel.DataAnnotations;

namespace ChatService.Model;

public class Chat
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string ReceiverId { get; set; }
    
    public User User { get; set; }
    public string UserId { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();


}