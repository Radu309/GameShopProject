using Microsoft.AspNetCore.Mvc;

namespace UserService.Controllers;

public class AuthController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}