using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingService.Models;
using ShoppingService.Service;

namespace ShoppingService.Controllers;

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
        Console.WriteLine("\t\tHEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE 1");
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(game);
        }
        Console.WriteLine("\t\tHEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE 2");

        await _gamesService.AddGameAsync(game);
        Console.WriteLine("\t\tHEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE 3");

        if (!await _gamesService.UploadImagesAsync(game.Id, imageFiles))
        {
            Console.WriteLine("\t\tHEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE4");

            ModelState.AddModelError("", "Failed to upload images.");
            return View(game);
        }
        Console.WriteLine("\t\tHEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE 5");

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
