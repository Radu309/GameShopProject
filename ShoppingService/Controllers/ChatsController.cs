using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingService.Data;
using ShoppingService.Models;
using ShoppingService.Models.Enum;

namespace ShoppingService.Controllers
{
    [Authorize]
    [Route("chat")]
    public class ChatsController : Controller
    {
        private readonly ShoppingDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChatsController(ShoppingDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> ChatInit()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return Unauthorized();

            ViewBag.Sender = null;
            ViewBag.Receiver = null;
            var users = _context.Users.ToList();

            var filteredUsers = await GetFilteredUsersByRole(users, User.IsInRole(Roles.Admin.ToString()));
            ViewBag.isAdmin = User.IsInRole(Roles.Admin.ToString());
            
            return View(filteredUsers);
        }

        [HttpGet]
        [Route("{receiverId}")]
        public async Task<IActionResult> ChatOneUser(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId)) return BadRequest("User ID is required.");

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null) return NotFound("User not found.");

            var senderEmail = User.FindFirstValue(ClaimTypes.Email);
            if (senderEmail == null) return Unauthorized();

            var sender = await _userManager.FindByEmailAsync(senderEmail);
            if (sender == null) return Unauthorized();

            ViewBag.Receiver = receiver;
            ViewBag.Sender = sender;
            // ViewBag.Messages = GetDemoMessages();

            var users = _context.Users.ToList();
            var filteredUsers = await GetFilteredUsersByRole(users, User.IsInRole(Roles.Admin.ToString()));
            ViewBag.isAdmin = User.IsInRole(Roles.Admin.ToString());
            
            return View(filteredUsers);
        }

        private async Task<List<User>> GetFilteredUsersByRole(List<User> users, bool isAdmin)
        {
            var role = isAdmin ? Roles.Client.ToString() : Roles.Admin.ToString();
            var filteredUsers = new List<User>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    filteredUsers.Add(user);
                }
            }

            return filteredUsers;
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
