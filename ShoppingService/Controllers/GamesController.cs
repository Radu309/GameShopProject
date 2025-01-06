using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoppingService.Models;
using ShoppingService.Service;

namespace ShoppingService.Controllers;

[Authorize] 
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
        var games = _gamesService.GetAllGames();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();
        ViewBag.CurrentUserId = userId;

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
            return View(game);
        
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

    // Get
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
