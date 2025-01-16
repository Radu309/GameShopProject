using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using GrpcService;

namespace ShoppingService.Controllers;

public class HomeController : Controller
{

    public async Task<IActionResult> Unary()
    {
        var channel = GrpcChannel.ForAddress("https://localhost:7225");
        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SendStatusAsync(new SRequest { No = 3 });
        return View("ShowStatus", (object)ChangetoDictionary(reply));
    }
    private Dictionary<string, string> ChangetoDictionary(SResponse response)
    {
        Dictionary<string, string> statusDict = new Dictionary<string, string>();
        foreach (StatusInfo status in response.StatusInfo)
            statusDict.Add(status.Author, status.Description);
        return statusDict;
    }
    
    public async Task<IActionResult> ServerStreaming()
    {
        var channel = GrpcChannel.ForAddress("https://localhost:7225");
        var client = new Greeter.GreeterClient(channel);
        Dictionary<string, string> statusDict = new Dictionary<string, string>();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        using (var call = client.ServerStreamToClient(new SRequest {}, cancellationToken: cts.Token))
        {
            try
            {
                await foreach (var message in call.ResponseStream.ReadAllAsync())
                {
                    statusDict.Add(message.StatusInfo[0].Author,
                        message.StatusInfo[0].Description);
                }
            }
            catch (RpcException ex) when (ex.StatusCode ==
                                          Grpc.Core.StatusCode.Cancelled)
            {
                // Log Stream cancelled
            }
        }
        return View("ShowStatus", (object)statusDict);
    }

    public async Task<IActionResult> ClientStreaming()
    {
        var channel = GrpcChannel.ForAddress("https://localhost:7225");
        var client = new Greeter.GreeterClient(channel);
        Dictionary<string, string> statusDict = new Dictionary<string, string>();
        int[] statuses = { 3, 2, 4 };
        using (var call = client.ClientStreamToServer())
        {
            foreach (var sT in statuses)
            {
                await call.RequestStream.WriteAsync(new SRequest { No = sT });
            }
            await call.RequestStream.CompleteAsync();
            SResponse sRes = await call.ResponseAsync;
            foreach (StatusInfo status in sRes.StatusInfo)
                statusDict.Add(status.Author, status.Description);
        }
        return View("ShowStatus", (object)statusDict);
    }
    public async Task<IActionResult> BiDirectionalStreaming()
    {
        var channel = GrpcChannel.ForAddress("https://localhost:7225");
        var client = new Greeter.GreeterClient(channel);
        Dictionary<string, string> statusDict = new Dictionary<string, string>();
        using (var call = client.BiDirectionalStreaming())
        {
            var responseReaderTask = Task.Run(async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var response = call.ResponseStream.Current;
                    foreach (StatusInfo status in response.StatusInfo)
                        statusDict.Add(status.Author, status.Description);
                }
            });
            int[] statusNo = { 3, 2, 4 };
            foreach (var sT in statusNo)
            {
                await call.RequestStream.WriteAsync(new SRequest { No = sT });
            }
            await call.RequestStream.CompleteAsync();
            await responseReaderTask;
        }
        return View("ShowStatus", (object)statusDict);
    }

}