using Microsoft.AspNetCore.Mvc;

namespace SaigonRide.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(); 
        }
    }
}