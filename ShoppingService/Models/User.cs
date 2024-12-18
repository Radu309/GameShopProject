using System.Text.Json.Serialization;

namespace ShoppingService.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    [JsonIgnore]
    public Cart? Cart { get; set; }
}