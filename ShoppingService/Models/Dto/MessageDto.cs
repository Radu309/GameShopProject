namespace ShoppingService.Models.Dto;

public class MessageDto
{
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}