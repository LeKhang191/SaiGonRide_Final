using Microsoft.AspNetCore.Mvc;

public class StationController : Controller
{
    public IActionResult Index()
    {
        return View(); 
    }
}