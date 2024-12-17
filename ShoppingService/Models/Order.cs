﻿namespace ShoppingService.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
}