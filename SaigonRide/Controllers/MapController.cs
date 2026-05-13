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

        [HttpGet]
        public IActionResult GetStations()
        {
            var stations = _context.Stations
                .Where(s => !s.IsDeleted)
                .Select(s => new {
                    s.StationId,
                    s.Name,
                    s.Latitude,
                    s.Longitude,
                    s.CurrentInventory,
                    s.MaxCapacity,
                    Status = s.CurrentInventory == 0 ? "Empty" : (s.IsLowInventory ? "Low" : "Active")
                })
                .ToList();

            return Json(stations);
        }
    }
}
