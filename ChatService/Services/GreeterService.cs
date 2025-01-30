using System.Runtime.InteropServices.JavaScript;
using Grpc.Core;
using ChatService;
using ChatService.Model;

namespace ChatService.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private static readonly List<Message> Messages = new List<Message>();

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<Message> ClientServer(Request request, ServerCallContext context)
    {
        var response = new Message
        {
            Success = true,
            Sender = request.Sender,
            Receiver = request.Receiver,
            Text = $"Message from {request.Sender} to {request.Receiver} received.",
            Timestamp = request.Timestamp // Folosim timestamp-ul primit de la client
        };

        Messages.Add(response);
        return Task.FromResult(response);
    }

    // Server Streaming - Trimite mesaje către client în mod continuu
    public override async Task ServerStreamToClient(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        foreach (var msg in Messages)
        {
            if (msg.Receiver == request.Receiver)
            {
                await responseStream.WriteAsync(msg);
                await Task.Delay(1000); // Simulăm o întârziere între mesaje
            }
        }
    }

    // Client Streaming - Clientul trimite mai multe mesaje către server, iar serverul răspunde o singură dată
    public override async Task<Message> ClientStreamToServer(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
    {
        int count = 0;
        string lastSender = "";
        string lastReceiver = "";
        JSType.Date lastTimestamp = null;

        while (await requestStream.MoveNext())
        {
            Messages.Add(new Message
            {
                Success = true,
                Sender = requestStream.Current.Sender,
                Receiver = requestStream.Current.Receiver,
                Text = requestStream.Current.Text,
                Timestamp = requestStream.Current.Timestamp // Folosim timestamp-ul din request
            });

            lastSender = requestStream.Current.Sender;
            lastReceiver = requestStream.Current.Receiver;
            lastTimestamp = requestStream.Current.Timestamp;
            count++;
        }

        return new Message
        {
            Success = true,
            Sender = "Server",
            Receiver = lastReceiver,
            Text = $"Received {count} messages from {lastSender}.",
            Timestamp = lastTimestamp // Răspundem cu ultimul timestamp primit
        };
    }

    // Bi-Directional Streaming - Serverul și clientul schimbă mesaje simultan
    public override async Task BiDirectionalStreaming(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            var response = new Message
            {
                Success = true,
                Sender = requestStream.Current.Sender,
                Receiver = requestStream.Current.Receiver,
                Text = $"Processed message: {requestStream.Current.Text}",
                Timestamp = requestStream.Current.Timestamp // Folosim timestamp-ul din request
            };

            await responseStream.WriteAsync(response);
        }
    }
    
}