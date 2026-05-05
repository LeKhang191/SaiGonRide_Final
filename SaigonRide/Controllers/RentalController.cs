using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Data;
using SaigonRide.Models.Entities;

namespace SaigonRide.Controllers
{
    public class RentalController : Controller
    {
        private readonly AppDbContext _context;

        public RentalController(AppDbContext context)
        {
            _context = context;
        }

        // READ
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rental
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            return View(rentals);
        }

        // CREATE - GET
        public IActionResult StartRental()
        {
            PopulateDropdowns();
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartRental(int userId, int vehicleId, int startStationId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);

            if (vehicle == null || vehicle.Status != "Available")
            {
                ModelState.AddModelError("", "Vehicle is not available or does not exist.");
                PopulateDropdowns();
                return View();
            }

            var rental = new Rental
            {
                UserId = userId,
                VehicleId = vehicleId,
                StartStationId = startStationId,
                StartTime = DateTime.Now,
                Status = "Active"
            };

            vehicle.Status = "In-Transit";

            _context.Rental.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Trip started successfully!";
            return RedirectToAction(nameof(Index));
        }

        // UPDATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndRental(int rentalId, int endStationId, string paymentMethod)
        {
            var rental = await _context.Rental
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.RentalId == rentalId && r.Status == "Active");

            if (rental == null) return NotFound();

            DateTime endTime = DateTime.Now;
            var duration = (endTime - rental.StartTime).TotalMinutes;

            double rate = (rental.Vehicle?.Type == "Electric Bike") ? 1500 : 500;
            double baseFare = Math.Max(1, duration) * rate;

            rental.EndTime = endTime;
            rental.EndStationId = endStationId;
            rental.BaseFare = baseFare;
            rental.FinalFare = baseFare;
            rental.PaymentMethod = paymentMethod;
            rental.Status = "Completed";

            rental.Vehicle.Status = "Available";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Receipt), new { id = rental.RentalId });
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var rental = await _context.Rental
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null) return NotFound();
            return View(rental);
        }

        private void PopulateDropdowns()
        {
            ViewBag.Users = new SelectList(_context.Users.ToList(), "UserId", "FullName");

            ViewBag.Vehicles = new SelectList(
                _context.Vehicles.Where(v => v.Status == "Available")
                .Select(v => new { v.VehicleId, Display = v.VehicleNumber + " - " + v.Type })
                .ToList(), "VehicleId", "Display");

            ViewBag.Stations = new SelectList(_context.Stations.ToList(), "StationId", "Name");
        }
    }
}