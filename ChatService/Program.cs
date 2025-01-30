using Microsoft.AspNetCore.Server.Kestrel.Core;
using ChatService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7225, o =>
    {
        o.Protocols = HttpProtocols.Http2;
        o.UseHttps();
    });
});

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GreeterService>();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();