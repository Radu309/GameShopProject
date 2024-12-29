using System.ComponentModel.DataAnnotations;

namespace ShoppingService.Models;

public class Game
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Please enter the game name.")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Please enter a description.")]
    public string Description { get; set; }
    [Required(ErrorMessage = "Please specify the price.")]
    public decimal Price { get; set; }
    [Required(ErrorMessage = "Please specify the stock.")]
    public int Stock { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
}