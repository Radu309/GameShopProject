using System.Text.Json.Serialization;

namespace ShoppingService.Models;

public class Cart
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; } = 0;
    public int UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    public int? OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}