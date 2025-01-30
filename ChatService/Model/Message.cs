namespace ChatService.Model;

public class Message
{
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public string Text { get; set; }
    public bool DeleteForYou { get; set; }
    public bool DeleteForEveryone { get; set; }
    public DateTime Timestamp { get; set; }
}