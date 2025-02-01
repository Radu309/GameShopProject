using Microsoft.AspNetCore.SignalR;
using ShoppingService.Models.Dto;

namespace ShoppingService.Hubs;

public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessageToAll(string user, string message)
    {
        Console.WriteLine($"Received message from {user}: {message}");
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    public async Task SendMessageToOne(string senderId, string receiverId, string message)
    {
        MessageDto messageDto = new MessageDto();
        messageDto.SenderId = senderId; 
        messageDto.ReceiverId = receiverId;
        messageDto.Message = message;
        messageDto.Timestamp = DateTime.UtcNow;
        await Clients.User(senderId).SendAsync("ReceiveMessage", messageDto);
        await Clients.User(receiverId).SendAsync("ReceiveMessage", messageDto);
    }
}