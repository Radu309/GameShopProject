namespace ShoppingService.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
}