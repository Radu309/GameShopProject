using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingService.Data;
using ShoppingService.Models;

namespace ShoppingService.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ShoppingDbContext _context;

        public OrdersController(ShoppingDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var shoppingDbContext = _context.Orders.Include(o => o.User);
            return View(await shoppingDbContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.CartItems)
                .ThenInclude(ci => ci.Games)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cartId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Games)
                    .FirstOrDefaultAsync(c => c.Id == cartId);
                
                if (cart != null)
                {
                    Order order = new Order
                    {
                        Date = DateTime.UtcNow,
                        UserId = cart.UserId,
                        TotalAmount = cart.TotalAmount,
                        CartItems = new List<CartItem>()
                    };

                    foreach (var cartItem in cart.CartItems)
                    {
                        var game = await _context.Games.FindAsync(cartItem.Games.First().Id);
                        if (game != null)
                        {
                            if (game.Stock < cartItem.Quantity)
                            {
                                ModelState.AddModelError("", $"Insufficient stock for game: {game.Name}");
                                await transaction.RollbackAsync();
                                return View("Error", new ErrorViewModel
                                {
                                    ErrorMessage = $"Insufficient stock for game: {game.Name}"
                                });
                            }

                            game.Stock -= cartItem.Quantity;
                            _context.Games.Update(game);
                        }

                        cartItem.CartId = null;
                        cartItem.OrderId = order.Id;
                        order.CartItems.Add(cartItem);
                    }

                    _context.Add(order);

                    cart.CartItems.Clear();
                    cart.TotalAmount = 0;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return RedirectToAction("Index", "Games");
                }

                return RedirectToAction("Index", "Games");
            }
            catch
            {
                await transaction.RollbackAsync();
                return View("Error", new ErrorViewModel
                {
                    ErrorMessage = "An error occurred while creating the order."
                });
            }
        }
    }
}
