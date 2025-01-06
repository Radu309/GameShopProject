using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingService.Data;
using ShoppingService.Models;

namespace ShoppingService.Controllers;

public class CartsController : Controller
{
    private readonly ShoppingDbContext _context;

    public CartsController(ShoppingDbContext context)
    {
        _context = context;
    }

    [HttpPost("Carts/AddGameToCart")]
    public async Task<IActionResult> AddGameToCart(string userId, int gameId, int quantity)
    {
        var user = await _context.Users
            .Include(u => u.Cart)
            .ThenInclude(c => c.CartItems)
            .ThenInclude(ci => ci.Games)
            .FirstOrDefaultAsync(u => u.Id  == userId);

        if (user == null)
            return NotFound("User not found.");

        var game = await _context.Games.FindAsync(gameId);
        if (game == null)
            return NotFound("Game not found.");

        if (user.Cart == null)
        {
            user.Cart = new Cart { UserId = userId.ToString() };
            _context.Carts.Add(user.Cart);
            await _context.SaveChangesAsync();
        }

        var cartItem = user.Cart.CartItems.FirstOrDefault(ci => ci.Games.Any(g => g.Id == gameId));
        if (cartItem != null)
        {
            cartItem.Quantity += quantity;
            cartItem.TotalPrice = cartItem.Quantity * game.Price; 
        }
        else
        {
            cartItem = new CartItem
            {
                Quantity = quantity,
                TotalPrice = quantity * game.Price,
                CartId = user.Cart.Id,
                OrderId = null,
                Games = new List<Game> { game }
            };
            user.Cart.CartItems.Add(cartItem);
        }
        user.Cart.TotalAmount = user.Cart.CartItems.Sum(ci => ci.TotalPrice);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Games");
    }
    
    [HttpGet("Carts/ViewCart/{userId}")]
    public async Task<IActionResult> ViewCart(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Cart)
            .ThenInclude(c => c.CartItems)
            .ThenInclude(ci => ci.Games)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound("User not found.");

        return View(user.Cart);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, string operation)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Games)
            .FirstOrDefaultAsync(c => c.CartItems.Any(ci => ci.Id == cartItemId));

        if (cart == null)
            return NotFound();
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
        if (cartItem == null)
            return NotFound();
        var game = cartItem.Games.FirstOrDefault();
        if (game == null)
            return NotFound();

        if (operation == "increment")
            cartItem.Quantity++;
        else if (operation == "decrement" && cartItem.Quantity > 1)
            cartItem.Quantity--;

        cartItem.TotalPrice = game.Price * cartItem.Quantity;
        cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(ViewCart), new { userId = cart.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem(int cartItemId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Games)
            .FirstOrDefaultAsync(c => c.CartItems.Any(ci => ci.Id == cartItemId));

        if (cart == null)
            return NotFound();
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
        if (cartItem == null )
            return NotFound();

        _context.CartItems.Remove(cartItem);
        cartItem.TotalPrice = 0;
        cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(ViewCart), new { userId = cart.UserId });
    }
}
