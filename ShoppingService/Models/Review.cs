using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ShoppingService.Models;

public class Review
{
    public int Id { get; set; }
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }
    [StringLength(200, MinimumLength = 0, ErrorMessage = "Comment must be between 0 and 200 characters.")]
    public string Comment { get; set; }
    public DateTime ReviewDate { get; set; }
    [Required]
    public string UserId { get; set; }
    [JsonIgnore] 
    public User? User { get; set; }
    [Required]
    public int GameId { get; set; }
    [JsonIgnore] 
    public Game? Game { get; set; }
}