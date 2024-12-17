namespace ShoppingService.Models;

public class User
{
    public int Id { get; set; }

    public ICollection<Order> Orders { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public Cart Cart { get; set; }
}