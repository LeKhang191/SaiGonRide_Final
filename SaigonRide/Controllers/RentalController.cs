using Microsoft.AspNetCore.Mvc;

namespace SaigonRide.Web.Controllers
{
    public class RentalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}