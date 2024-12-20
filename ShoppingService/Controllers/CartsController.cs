using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingService.Data;
using ShoppingService.Models;

namespace ShoppingService.Controllers
{
    public class CartsController : Controller
    {
        private readonly ShoppingDbContext _context;

        public CartsController(ShoppingDbContext context)
        {
            _context = context;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var shoppingDbContext = _context.Carts.
                Include(c => c.User)
                .Include(c => c.Order);
            return View(await shoppingDbContext.ToListAsync());
        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        [HttpPost("Carts/AddGameToCart")]
        public async Task<IActionResult> AddGameToCart(int userId, int gameId, int quantity)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .ThenInclude(c => c.CartItems)
                .ThenInclude(ci => ci.Games)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
                return NotFound("Game not found.");

            if (user.Cart == null)
            {
                user.Cart = new Cart { UserId = userId };
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
                    Games = new List<Game> { game }
                };
                user.Cart.CartItems.Add(cartItem);
            }
            user.Cart.TotalAmount = user.Cart.CartItems.Sum(ci => ci.TotalPrice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Games");
        }
        
        [HttpGet("Carts/ViewCart/{userId}")]
        public async Task<IActionResult> ViewCart(int userId)
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
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null || cartItem.Cart == null)
                return NotFound();

            _context.CartItems.Remove(cartItem);
            cartItem.Cart.TotalAmount = cartItem.Cart.CartItems.Sum(ci => ci.TotalPrice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ViewCart), new { userId = cartItem.Cart.UserId });
        }

        
        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TotalAmount,UserId,OrderId")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", cart.OrderId);

            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", cart.OrderId);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TotalAmount,UserId,OrderId")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", cart.OrderId);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}
