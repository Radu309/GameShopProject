﻿using Microsoft.AspNetCore.SignalR;

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
    public async Task SendMessageToOne(string sender, string receiver, string message)
    {
        
        Console.WriteLine( );
        Console.WriteLine($"Log: {receiver} received message from {sender}: {message}");
        await Clients.User(sender).SendAsync("ReceiveMessage", sender, message);
        await Clients.User(receiver).SendAsync("ReceiveMessage", sender, message);
    }
}