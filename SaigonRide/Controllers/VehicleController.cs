using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using System.Linq;

namespace SaigonRide.Controllers
{
    public class VehicleController : Controller
    {
        private readonly AppDbContext _context;

        public VehicleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Vehicle 
        public IActionResult Index(string status, int? stationId)
        {
            var query = _context.Vehicles.Include(v => v.Station).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(v => v.Status == status);
            }

            if (stationId.HasValue)
            {
                query = query.Where(v => v.StationId == stationId);
            }

            ViewBag.Stations = new SelectList(_context.Stations, "StationId", "Name");

            return View(query.ToList());
        }

        // GET: Vehicle/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Stations = new SelectList(_context.Stations, "StationId", "Name");
            return View();
        }

        // POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                _context.Vehicles.Add(vehicle);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Stations = new SelectList(_context.Stations, "StationId", "Name", vehicle.StationId);
            return View(vehicle);
        }

        // GET: Vehicle/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle == null) return NotFound();

            ViewBag.StationId = new SelectList(_context.Stations, "StationId", "Name", vehicle.StationId);
            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Vehicle vehicle)
        {
            if (id != vehicle.VehicleId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehicle);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehicles.Any(e => e.VehicleId == id)) return NotFound();
                    else throw;
                }
            }
            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}