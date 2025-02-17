using Grpc.Core;
using GrpcService;

namespace GrpcService.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }
    
    public override Task<SResponse> SendStatus(SRequest request, ServerCallContext context)
    {
        List<StatusInfo> statusList = StatusRepo();
        SResponse sRes = new SResponse();
        sRes.StatusInfo.AddRange(statusList.Skip(request.No - 1).Take(1));
        return Task.FromResult(sRes);
    }
    public override async Task ServerStreamToClient(SRequest request, 
        IServerStreamWriter<SResponse> responseStream, ServerCallContext context)
    {
        List<StatusInfo> statusList = StatusRepo();
        SResponse sRes;
        var i = 0;
        while (!context.CancellationToken.IsCancellationRequested)
        {
            sRes = new SResponse();
            sRes.StatusInfo.Add(statusList.Skip(i).Take(1));
            await responseStream.WriteAsync(sRes);
            i++;

            await Task.Delay(1000);
        }
    }
    public override async Task<SResponse> ClientStreamToServer(IAsyncStreamReader<SRequest>
        requestStream, ServerCallContext context)
    {
        List<StatusInfo> statusList = StatusRepo();
        SResponse sRes = new SResponse();
        await foreach (var message in requestStream.ReadAllAsync())
        {
            sRes.StatusInfo.Add(statusList.Skip(message.No - 1).Take(1));
        }
        return sRes;
    }
    public override async Task BiDirectionalStreaming(IAsyncStreamReader<SRequest> requestStream,
        IServerStreamWriter<SResponse> responseStream, ServerCallContext context)
    {
        List<StatusInfo> statusList = StatusRepo();
        SResponse sRes;
        await foreach (var message in requestStream.ReadAllAsync())
        {
            sRes = new SResponse();
            sRes.StatusInfo.Add(statusList.Skip(message.No - 1).Take(1));
            await responseStream.WriteAsync(sRes);
        }
    }
    
    private List<StatusInfo> StatusRepo()
    {
        List<StatusInfo> statusList = new List<StatusInfo> {
            new StatusInfo { Author = "Randy", Description = "Task 1 in progress"},
            new StatusInfo { Author = "John", Description = "Task 1 just started"},
            new StatusInfo { Author = "Miriam", Description = "Finished all tasks"},
            new StatusInfo { Author = "Petra", Description = "Task 2 finished"},
            new StatusInfo { Author = "Steve", Description = "Task 2 in progress"}
        };
        return statusList;
    }
    



}