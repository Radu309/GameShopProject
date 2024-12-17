using System.Text.Json.Serialization;

namespace UserService.Models;

public class Review
{
    public int Id { get; set; }
    public string UserId { get; set; }
    [JsonIgnore] 
    public User User { get; set; }
}