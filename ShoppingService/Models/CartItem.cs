namespace ShoppingService.Models;

public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; }
}