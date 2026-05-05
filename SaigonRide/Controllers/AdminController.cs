using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;
using System.Linq;

namespace SaigonRide.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            ViewBag.TotalVehicles = _context.Vehicles.Count();
            ViewBag.TotalStations = _context.Stations.Count();
            ViewBag.AvailableBikes = _context.Vehicles.Count(v => v.Status == "Available");
            ViewBag.TotalUsers = _context.Users.Count();

            return View();
        }

        public IActionResult RevenueReport(DateTime? startDate, DateTime? endDate)
        {
            var transactions = _context.Transactions.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                transactions = transactions.Where(t => t.PaymentDate >= startDate && t.PaymentDate <= endDate);
            }

            ViewBag.TotalRevenue = transactions.Sum(t => t.TotalFare);
            return View(transactions.ToList());
        }
    }
}