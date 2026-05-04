using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;
using System.Linq;

namespace SaigonRide.Controllers
{
    public class StationController : Controller
    {
        private readonly AppDbContext _context;

        public StationController(AppDbContext context)
        {
            _context = context;
        }

        // Station List
        public IActionResult Index()
        {
            var stations = _context.Stations.ToList();
            return View(stations);
        }
    }
}