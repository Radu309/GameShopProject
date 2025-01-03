using System.Text.Json.Serialization;

namespace ShoppingService.Models;

public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public int? CartId { get; set; }
    [JsonIgnore]
    public Cart? Cart { get; set; }
    public int? OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }

    public ICollection<Game> Games { get; set; } = new List<Game>();

}