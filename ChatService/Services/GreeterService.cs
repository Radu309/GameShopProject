using System.Runtime.InteropServices.JavaScript;
using Grpc.Core;
using ChatService;
using ChatService.Data;
using ChatService.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private readonly ChatDbContext _context;

    public GreeterService(ILogger<GreeterService> logger, ChatDbContext dbContext)
    {
        _logger = logger;
        _context = dbContext;
    }

    public override async Task<Check> CheckConnection(Check request, ServerCallContext context)
    {
        var response = new Check
        {
            Success = true
        };
        return response;
    }
    
    public override async Task<Check> SaveOnePrivateMessage(MessageRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received message from {SenderId} to {ReceiverId}: {Content}", 
            request?.SenderId, request?.ReceiverId, request?.Content);

        var sender = await _context.Users.FindAsync(request.SenderId);
        var receiver = await _context.Users.FindAsync(request.ReceiverId);

        if (sender == null || receiver == null)
        {
            _logger.LogWarning("User not found: SenderId={SenderId}, ReceiverId={ReceiverId}", request.SenderId, request.ReceiverId);
            return new Check { Success = false };
        }

        var chat = await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.UserId == sender.Id || c.UserId == receiver.Id);

        if (chat == null)
        {
            chat = new Chat {User = sender, UserId = sender.Id, ReceiverId = receiver.Id, Messages = new List<Message>() };
            _context.Chats.Add(chat);  

            _logger.LogInformation("Created new chat between {SenderId} and {ReceiverId}", sender.Id, receiver.Id);
        }

        var message = new Message
        {
            UserId = sender.Id, 
            Content = request.Content, 
            Chat = chat, ChatId = chat.Id,
        };

        chat.Messages.Add(message);
        _context.Messages.Add(message);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Message successfully saved: {Content}", message.Content);
        return new Check { Success = true };
    }

    public override async Task GetChatMessages(ChatRequest request,
        IServerStreamWriter<ChatResponse> responseStream,
        ServerCallContext context)
    {
        var chat = await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c =>
                (c.UserId == request.SenderId && c.ReceiverId == request.ReceiverId) ||
                (c.UserId == request.ReceiverId && c.ReceiverId == request.SenderId));
        if (chat == null || chat.Messages == null || !chat.Messages.Any())
        {
            _logger.LogWarning($"No chat found between {request.SenderId} and {request.ReceiverId}");
            return;
        }

        var messages = _context.Messages
            .Where(m => m.ChatId == chat.Id && !m.DeleteForEveryone)
            .OrderBy(m => m.SentAt)
            .AsAsyncEnumerable();

        await foreach (var message in messages.WithCancellation(context.CancellationToken))
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Client disconnected before receiving all messages.");
                break;
            }
            var response = new ChatResponse
            {
                Content = message.Content, 
                UserId = message.UserId,
                DeleteForYou = message.DeleteForYou,
                DeleteForEveryone = message.DeleteForEveryone,
                IsRead = message.IsRead,
                SentAt = new SentAt
                {
                    Year = message.SentAt.Year,
                    Month = message.SentAt.Month,
                    Day = message.SentAt.Day,
                    Hour = message.SentAt.Hour,
                    Minute = message.SentAt.Minute,
                    Second = message.SentAt.Second
                }
            };

            await responseStream.WriteAsync(response);
            _logger.LogInformation($"Sent message: {message.Content}");
        }
    }

    public override async Task<Check> CreateUser(UserIdRequest userIdRequest, ServerCallContext context)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdRequest.Id);
        if (user == null)
        {
            User newUser = new User() { Id = userIdRequest.Id };
            _context.Add(newUser);
            await _context.SaveChangesAsync();
            return new Check { Success = true };
        }
        return new Check { Success = false };
    }
    
    public override async Task<Check> DeleteUser(UserIdRequest userIdRequest, ServerCallContext context)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdRequest.Id);
        if (user != null)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return new Check { Success = true };
        }
        return new Check { Success = false };
    }
}