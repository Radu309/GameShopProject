using System.Security.Claims;
using ChatService;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShoppingService.Data;
using ShoppingService.Models;
using ShoppingService.Models.Dto;
using ShoppingService.Models.Enum;

namespace ShoppingService.Controllers
{
    [Authorize]
    [Route("chat")]
    public class ChatsController : Controller
    {
        private readonly ShoppingDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly GrpcChannel _grpcChannel;

        public ChatsController(ShoppingDbContext context, 
            UserManager<User> userManager,
            GrpcChannel grpcChannel)
        {
            _context = context;
            _userManager = userManager;
            _grpcChannel = grpcChannel ?? throw new ArgumentNullException(nameof(grpcChannel));
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> ChatInit()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
                return View("Error", new ErrorViewModel { ErrorMessage = "Unauthorized access." });

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
                return View("Error", new ErrorViewModel { ErrorMessage = "User not found." });
            
            ViewBag.Sender = user;
            ViewBag.Receiver = null;

            var filteredUsers = await GetFilteredUsersByRole();
            ViewBag.isAdmin = User.IsInRole(Roles.Admin.ToString());
            
            return View(filteredUsers);
        }

        [HttpGet]
        [Route("{receiverId}")]
        public async Task<IActionResult> ChatOneUser(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId)) 
                return View("Error", new ErrorViewModel { ErrorMessage = "User ID is required." });

            var senderEmail = User.FindFirstValue(ClaimTypes.Email);
            if (senderEmail == null)
                return View("Error", new ErrorViewModel { ErrorMessage = "Unauthorized access." });

            var sender = await _userManager.FindByEmailAsync(senderEmail);
            if (sender == null)
                return View("Error", new ErrorViewModel { ErrorMessage = "User not found." });

            if (sender.Id == receiverId)
                return View("Error", new ErrorViewModel { ErrorMessage = "You cannot chat with yourself." });

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null)
                return View("Error", new ErrorViewModel { ErrorMessage = "User not found." });

            if (await HaveSameRoleAsync(sender, receiver))
                return View("Error", new ErrorViewModel { ErrorMessage = "You cannot chat with a user that has the same role as you." });
            
            ViewBag.Receiver = receiver;
            ViewBag.Sender = sender;
            
            var request = new ChatRequest() { SenderId = sender.Id, ReceiverId = receiver.Id };
            var client = new Greeter.GreeterClient(_grpcChannel);
            using var call = client.GetChatMessages(request);
            List<ChatResponse> messages = new List<ChatResponse>();

            await foreach (var message in call.ResponseStream.ReadAllAsync())
            {
                messages.Add(message);
            }
            ViewBag.Messages = messages;
            ViewBag.isAdmin = User.IsInRole(Roles.Admin.ToString());

            var filteredUsers = await GetFilteredUsersByRole();
            return View(filteredUsers);
        }

        private async Task<List<User>> GetFilteredUsersByRole()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null) return new List<User>();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return new List<User>();

            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin.ToString());
            var targetRole = isAdmin ? Roles.Client.ToString() : Roles.Admin.ToString();

            return (await _userManager.GetUsersInRoleAsync(targetRole)).ToList();
        }

        private async Task<bool> HaveSameRoleAsync(User user1, User user2)
        {
            var rolesUser1 = await _userManager.GetRolesAsync(user1);
            var rolesUser2 = await _userManager.GetRolesAsync(user2);

            return rolesUser1.Intersect(rolesUser2).Any(); 
        }

        
        private List<object> GetDemoMessages()
        {
            return new List<object>
            {
                new { Text = "Hello!", Time = "10:00 AM", Sender = "other" },
                new { Text = "Hi there!", Time = "10:05 AM", Sender = "self" }
            };
        }
    }
}
