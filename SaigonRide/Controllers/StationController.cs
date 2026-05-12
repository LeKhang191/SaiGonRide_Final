using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
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

        // Read
        public IActionResult Index()
        {
            var stations = _context.Stations.Where(s => !s.IsDeleted).ToList();
            return View(stations);
        }

        // GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Create(Station station)
        {
            if (_context.Stations.Any(s => s.Name == station.Name))
            {
                ModelState.AddModelError("Name", "This station name already exists.");
                return View(station);
            }

            station.CurrentInventory = 0;
            _context.Stations.Add(station);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Update
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var station = _context.Stations.Find(id);
            if (station == null) return NotFound();
            return View(station);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Station station)
        {
            if (id != station.StationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(station);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StationExists(station.StationId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(station);
        }

        // Delete 
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var station = _context.Stations.Find(id);

            if (station == null) return NotFound();

            if (station.CurrentInventory > 0)
            {
                TempData["Error"] = "Cannot delete a station with vehicles. Please transfer them first!";
                return RedirectToAction("Index");
            }

            station.IsDeleted = true;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool StationExists(int id)
        {
            return _context.Stations.Any(e => e.StationId == id);
        }
    }
}

