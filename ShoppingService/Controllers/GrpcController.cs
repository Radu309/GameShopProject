using ChatService;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingService.Models.Dto;

namespace ShoppingService.Controllers;

// [Authorize]
[Route("api/grpc/")]
public class GrpcController : ControllerBase
{
    private readonly GrpcChannel channel;

    public GrpcController()
    {
        channel = GrpcChannel.ForAddress("https://localhost:7223");
    }
    
    [HttpPost]
    [Route("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
    {
        try
        {
            var client = new Greeter.GreeterClient(channel);
            var response = await client.SaveOnePrivateMessageAsync(request);
            if (!response.Success)
                return BadRequest("Can't send message");
            return Ok();
        }
        catch (Grpc.Core.RpcException ex)
        {
            return StatusCode(500, new { error = "Error sending private message", details = ex.Status.Detail });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred", details = ex.Message });
        }
    }
}