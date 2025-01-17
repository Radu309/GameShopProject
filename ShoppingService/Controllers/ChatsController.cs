using Microsoft.AspNetCore.Mvc;
using ShoppingService.Models.Dto;

namespace ShoppingService.Controllers;

public class ChatsController : Controller
{
    private static List<ChatDto> _messages = new List<ChatDto>();

    public IActionResult Index()
    {
        return View(_messages);
    }

    [HttpPost]
    public IActionResult AddMessage(ChatDto chatMessage)
    {
        if (ModelState.IsValid)
        {
            _messages.Add(chatMessage);
        }
        return RedirectToAction("Index");
    }
}