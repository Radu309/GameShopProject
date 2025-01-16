using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> UserConnections = new();

    public async Task RegisterUser(string userId)
    {
        UserConnections[userId] = Context.ConnectionId;
        await Clients.Caller.SendAsync("Registered", $"Utilizatorul {userId} a fost înregistrat cu succes.");
    }

    public async Task SendMessageToUser(string receiverId, string message)
    {
        if (UserConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", Context.ConnectionId, message);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", $"Utilizatorul cu ID-ul {receiverId} nu este conectat.");
        }
    }

    public override Task OnDisconnectedAsync(System.Exception exception)
    {
        var userId = UserConnections.FirstOrDefault(pair => pair.Value == Context.ConnectionId).Key;
        if (userId != null)
        {
            UserConnections.TryRemove(userId, out _);
        }
        return base.OnDisconnectedAsync(exception);
    }
}