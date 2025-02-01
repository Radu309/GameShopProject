using System.ComponentModel.DataAnnotations;

namespace ChatService.Model;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();

}