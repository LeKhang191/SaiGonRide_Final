using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;

namespace SaigonRide.Controllers
{
    public class MapController : Controller
    {
        private readonly AppDbContext _context;
        public MapController(AppDbContext context) => _context = context;

        public IActionResult Index()
        {
            var stations = _context.Stations.Where(s => !s.IsDeleted).ToList();
            return View(stations);
        }
    }
}
