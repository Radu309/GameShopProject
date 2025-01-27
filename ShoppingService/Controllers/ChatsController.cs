using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingService.Data;
using ShoppingService.Models;
using ShoppingService.Models.Enum;

namespace ShoppingService.Controllers;

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

    [Route("")]
    [HttpGet]
    public IActionResult Chat()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (userEmail == null)
            return Unauthorized();
        var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
        if (user == null)
            return Unauthorized();
        ViewBag.Sender = null;
        
        var users = _context.Users.ToList();
        
        if (User.IsInRole(Roles.Admin.ToString()))
        {
            var clients = new List<User>();
            foreach (var item in users)
            {
                if (_userManager.IsInRoleAsync(item, Roles.Client.ToString()).Result)
                    clients.Add(item);
            }
            ViewBag.isAdmin = true;
            return View(clients);
        }
        else
        {
            var admins = new List<User>();
            foreach (var item in users)
            {
                if (_userManager.IsInRoleAsync(item, Roles.Admin.ToString()).Result)
                    admins.Add(item);
            }
            ViewBag.isAdmin = false;
            return View(admins);
        }
    }
    [HttpGet]
    [Route("{receiverId}")]
    public IActionResult SelectUser(string receiverId)
    {
        if (string.IsNullOrEmpty(receiverId))
            return BadRequest("User ID is required.");

        var receiver = _context.Users.FirstOrDefault(u => u.Id == receiverId);
        if (receiver == null)
            return NotFound("User not found.");
        
        var senderEmail = User.FindFirstValue(ClaimTypes.Email);
        if (senderEmail == null)
            return Unauthorized();
        var sender = _context.Users.FirstOrDefault(u => u.Email == senderEmail);
        if (sender == null)
            return Unauthorized();

        // pentru demo
        var messages = new List<object>
        {
            new { Text = "Hello!", Time = "10:00 AM", Sender = "other" },
            new { Text = "Hi there!", Time = "10:05 AM", Sender = "self" }
        };

        ViewBag.Receiver = receiver;
        ViewBag.Sender = sender;
        ViewBag.Messages = messages;
        // return View("Chat", _context.Users.ToList());
        return PartialView("_ChatMain");
    }
}