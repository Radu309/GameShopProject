using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ShoppingService.Models;

public class Cart
{
    public int Id { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Total amount cannot be negative.")]
    public decimal TotalAmount { get; set; } = 0;
    [Required(ErrorMessage = "User ID is required.")]
    public string UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}