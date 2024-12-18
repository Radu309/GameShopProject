using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ShoppingService.Models;

public class Image
{
    public int Id { get; set; }
    [Required]
    public string Base64Data { get; set; }
    [Required]
    public string FileName { get; set; }

    public int GameId { get; set; }
    [JsonIgnore]
    public Game? Game { get; set; }
}