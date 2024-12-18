﻿using System.Text.Json.Serialization;

namespace ShoppingService.Models;

public class Cart
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }

    public int UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}