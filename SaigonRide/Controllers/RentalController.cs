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