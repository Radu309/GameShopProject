using System.Text.Json.Serialization;

namespace ShoppingService.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }

    public int UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}