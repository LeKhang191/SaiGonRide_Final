using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using SaigonRide.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SaigonRide.Controllers
{
    public class RentalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly IPayPalService _payPalService;

        public RentalController(AppDbContext context, IVnPayService vnPayService, IPayPalService payPalService)
        {
            _context = context;
            _vnPayService = vnPayService;
            _payPalService = payPalService;
        }

        // READ
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            return View(rentals);
        }

        // DELETE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return Forbid();
            }

            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Rental record deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // CREATE - GET
        public IActionResult StartRental(int? stationId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            PopulateDropdowns();
            if(stationId.HasValue)
            {
                ViewBag.StartStationId = stationId.Value;
            }
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartRental(int userId, int vehicleId, int startStationId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

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

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Trip started successfully!";
            return RedirectToAction(nameof(Index));
        }

        // UPDATE
        [HttpGet]
        public async Task<IActionResult> EndRental(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null) return NotFound();

            ViewBag.Stations = new SelectList(_context.Stations, "StationId", "Name");

            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndRental(int rentalId, int endStationId, string paymentMethod)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RentalId == rentalId && r.Status == "Active");

            if (rental == null) return NotFound();

            DateTime endTime = DateTime.Now;
            var duration = (endTime - rental.StartTime).TotalMinutes;
            duration = duration < 0 ? 120 : Math.Max(1, duration);
            double rate = (rental.Vehicle?.Type == "Electric Bike") ? 1500 : 500;
            double baseFare = Math.Max(1, duration) * rate;
            double co2Saved = Math.Max(1, duration) * 50;

            rental.EndTime = endTime;
            rental.EndStationId = endStationId;
            rental.BaseFare = baseFare;
            rental.FinalFare = baseFare;
            rental.Co2Saved = co2Saved;
            rental.PaymentMethod = paymentMethod;
            rental.Vehicle.Status = "Available";

            await _context.SaveChangesAsync();

            if (paymentMethod == "VNPay")
            {
                var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, rental.RentalId, baseFare);
                return Redirect(paymentUrl);
            }
            else if (paymentMethod == "PayPal")
            {
                double usdAmount = Math.Round(baseFare / 25400, 2);
                var url = await _payPalService.CreatePaymentUrl(rental.RentalId, usdAmount);

                if (!string.IsNullOrEmpty(url))
                {
                    return Redirect(url);
                }
            }

            if (rental.User != null)
            {
                rental.User.TotalCo2Saved += co2Saved;
            }

            rental.Status = "Completed";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Receipt), new { id = rental.RentalId });
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Error"] = "Payment failed or was canceled.";
                return RedirectToAction("Index");
            }

            var rentalId = int.Parse(response.OrderId);
            var rental = await _context.Rentals.FindAsync(rentalId);

            if (rental != null)
            {
                rental.Status = "Completed";
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Payment via VNPay successful!";
            return RedirectToAction("Receipt", new { id = rentalId });
        }

        [HttpPost]
        public async Task<IActionResult> PaymentWithPayPal(int rentalId)
        {
            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental == null) return NotFound();

            double usdAmount = Math.Round((double)rental.FinalFare / 25400, 2);

            var url = await _payPalService.CreatePaymentUrl(rentalId, usdAmount);
            return Redirect(url);
        }


        public async Task<IActionResult> PayPalCallback(string paymentId, string payerId, string token)
        {
            var result = await _payPalService.ExecutePayment(paymentId, payerId);

            if (result)
            {
                var rental = await _context.Rentals.OrderByDescending(r => r.EndTime).FirstOrDefaultAsync();

                if (rental != null)
                {
                    rental.Status = "Completed";
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "PayPal payment successful!";
                    return RedirectToAction("Receipt", new { id = rental.RentalId });
                }
            }

            TempData["Error"] = "PayPal payment failed!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult PaymentCancel()
        {
            TempData["Error"] = "Payment was canceled. You can try again.";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null) return NotFound();
            return View(rental);
        }

        public async Task<IActionResult> FastForward(int rentalId)
        {
            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental != null)
            {
                rental.StartTime = DateTime.Now.AddHours(-3);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("EndRental", new { id = rentalId });
        }

        private void PopulateDropdowns()
        {
            ViewBag.Users = new SelectList(_context.Users.Where(u => u.UserType != "Admin").ToList(), "UserId", "FullName");
            ViewBag.Stations = new SelectList(_context.Stations.Where(s => !s.IsDeleted).ToList(), "StationId", "Name");

            var vehicles = _context.Vehicles
                .Where(v => v.Status == "Available")
                .Select(v => new {
                    v.VehicleId,
                    Display = v.VehicleNumber + " - " + v.Type + (v.BatteryLevel.HasValue ? $" (Pin: {v.BatteryLevel}%)" : ""),
                    ImageUrl = v.ImageUrl
                })
                .ToList();

            ViewBag.Vehicles = new SelectList(vehicles, "VehicleId", "Display");
            ViewBag.VehicleData = vehicles;
        }

        [HttpGet]
        public async Task<IActionResult> MyRides()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myRides = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            return View(myRides);
        }

        [HttpGet]
        public async Task<IActionResult> Leaderboard()
        {
            var topUsers = await _context.Users
                .Where(u => u.UserType != "Admin" && u.TotalCo2Saved > 0)
                .OrderByDescending(u => u.TotalCo2Saved)
                .Take(10)
                .ToListAsync();

            return View(topUsers);
        }
    }
}