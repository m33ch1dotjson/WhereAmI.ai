using Microsoft.AspNetCore.Mvc;

namespace GeoSpy.ai.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
