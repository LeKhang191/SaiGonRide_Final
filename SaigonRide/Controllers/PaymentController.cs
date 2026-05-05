using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;
using SaigonRide.Models.Entities;

namespace SaigonRide.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Checkout(int vehicleId, double fare)
        {
            var vehicle = _context.Vehicles.Find(vehicleId);
            if (vehicle == null) return NotFound();

            ViewBag.VehicleId = vehicleId;
            ViewBag.VehicleNumber = vehicle.VehicleNumber;
            ViewBag.Fare = fare;

            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Transactions.Add(transaction);
                _context.SaveChanges();

                return RedirectToAction("Receipt", new { id = transaction.TransactionId });
            }
            return View("Checkout", transaction);
        }

        public IActionResult Receipt(int id)
        {
            var transaction = _context.Transactions.Find(id);
            if (transaction == null) return NotFound();
            return View(transaction);
        }
    }
}