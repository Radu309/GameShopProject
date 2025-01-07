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

[Authorize] 
public class ReviewsController : Controller
{
    private readonly ShoppingDbContext _context;

    public ReviewsController(ShoppingDbContext context)
    {
        _context = context;
    }

    // GET: Reviews
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Index()
    {
        var reviews = _context.Reviews.Include(r => r.Game).Include(r => r.User);
        return View(await reviews.ToListAsync());
    }

    // GET: Reviews/Details/5
    [Authorize(Policy = "AdminClientPolicy")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var review = await _context.Reviews
            .Include(r => r.Game)
            .Include(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (review == null)
        {
            return NotFound();
        }

        return View(review);
    }

    // GET: Reviews/Create
    [Authorize(Policy = "ClientPolicy")]
    public IActionResult Create(int? gameId)
    {
        if (gameId == null)
            return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();
        
        var review = new Review { GameId = gameId.Value, UserId = userId };
        return View(review);
    }

    // POST: Reviews/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ClientPolicy")]
    public async Task<IActionResult> Create([Bind("Rating,Comment,UserId,GameId")] Review review)
    {
        if (ModelState.IsValid)
        {
            review.ReviewDate = DateTime.UtcNow;
            _context.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Games");
        }
        foreach (var modelState in ModelState.Values)
        {
            foreach (var error in modelState.Errors)
            {
                Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }
        }
        ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id", review.GameId);
        ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", review.UserId);
        return View(review);
    }
    
    // GET: Reviews/Delete/5
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var review = await _context.Reviews
            .Include(r => r.Game)
            .Include(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (review == null)
            return NotFound();
        
        return View(review);
    }

    // POST: Reviews/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
            _context.Reviews.Remove(review);
        else
            return NotFound();
        
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ReviewExists(int id)
    {
        return _context.Reviews.Any(e => e.Id == id);
    }
}
