using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ShoppingService.Models.Enum;

namespace ShoppingService.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Email { get; set; }
    public Role Role { get; set; } 

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    [JsonIgnore]
    public Cart? Cart { get; set; }
}