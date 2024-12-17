using Microsoft.EntityFrameworkCore;
using ShoppingService.Data;
using ShoppingService.Models;

namespace ShoppingService.Service;

public class GamesService
{
    private readonly ShoppingDbContext _context;

    public GamesService(ShoppingDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Game> GetAllGames() =>
        _context.Games
            .Include(g => g.Images)
            .ToList();

    public async Task<Game?> GetGameByIdAsync(int id) =>
        await _context.Games
            .Include(g => g.Images)
            .Include(g => g.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<bool> UploadImagesAsync(int gameId, IFormFile[] files)
    {
        if (files == null || files.Length == 0) return false;

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());

                var image = new Image
                {
                    Base64Data = base64,
                    GameId = gameId,
                    FileName = file.FileName
                };
                _context.Images.Add(image);
            }
            else return false;
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AddGameAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateGameAsync(Game existingGame, Game updatedGame)
    {
        existingGame.Name = updatedGame.Name;
        existingGame.Description = updatedGame.Description;
        existingGame.Price = updatedGame.Price;
        existingGame.Stock = updatedGame.Stock;

        _context.Games.Update(existingGame);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGameAsync(int id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game != null)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteImageAsync(int imageId)
    {
        var image = await _context.Images.FindAsync(imageId);
        if (image != null)
        {
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
        }
    }
}
