using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoppingService.Models;
using ShoppingService.Service;

namespace ShoppingService.Controllers;

// [Authorize] 
// [Authorize(Policy = "ClientPolicy")]
public class GamesController : Controller
{
    private readonly GamesService _gamesService;

    public GamesController(GamesService gamesService)
    {
        _gamesService = gamesService;
    }

    [HttpGet]
    public IActionResult Index()
    {   
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value; 
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        Console.WriteLine("HEREEEEEEEEEEEEEEEEEE");
        Console.WriteLine(userId);
        Console.WriteLine(userRole);
        Console.WriteLine(userEmail);
        
        var games = _gamesService.GetAllGames();
        // TO DO: to add the logic for userId
        ViewBag.CurrentUserId = 1;

        // ViewBag.IsAdmin = User.IsInRole("Admin");
        return View(games);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var game = await _gamesService.GetGameByIdAsync(id.Value);
        return game != null ? View(game) : NotFound();
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Game game, IFormFile[] imageFiles)
    {
        if (!ModelState.IsValid)
        {
            // foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            // {
            //     Console.WriteLine(error.ErrorMessage);
            // }
            return View(game);
        }
        if (imageFiles.IsNullOrEmpty())
        {
            ModelState.AddModelError("", "Please upload at least one image.");
            return View(game);
        }
        await _gamesService.AddGameAsync(game);

        if (!await _gamesService.UploadImagesAsync(game.Id, imageFiles))
        {
            ModelState.AddModelError("", "Failed to upload images.");
            return View(game);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var game = await _gamesService.GetGameByIdAsync(id.Value);
        return game != null ? View(game) : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Game updatedGame, IFormFile[] imageFiles)
    {
        if (!ModelState.IsValid)
            return View(updatedGame);
        var existingGame = await _gamesService.GetGameByIdAsync(id);
        if (existingGame == null) return NotFound();

        await _gamesService.UpdateGameAsync(existingGame, updatedGame);
        if (!await _gamesService.UploadImagesAsync(id, imageFiles))
        {
            ModelState.AddModelError("", "Failed to upload images.");
            return View(existingGame);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(int imageId, int gameId)
    {
        await _gamesService.DeleteImageAsync(imageId);
        return RedirectToAction(nameof(Edit), new { id = gameId });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var game = await _gamesService.GetGameByIdAsync(id.Value);
        return game != null ? View(game) : NotFound();
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _gamesService.DeleteGameAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
