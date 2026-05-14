using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalVehicles = _context.Vehicles.Count();
            ViewBag.TotalStations = _context.Stations.Count();
            ViewBag.AvailableBikes = _context.Vehicles.Count(v => v.Status == "Available");
            ViewBag.TotalUsers = _context.Users.Count();

            var vehicleStatusData = _context.Vehicles
                .GroupBy(v => v.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            ViewBag.VehicleStatusLabels = vehicleStatusData.Select(x => x.Status).ToList();
            ViewBag.VehicleStatusCounts = vehicleStatusData.Select(x => x.Count).ToList();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.Date.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var revenueData = _context.Rentals
                .Where(r => r.EndTime != null && r.EndTime >= last7Days.First())
                .GroupBy(r => r.EndTime.Value.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(r => r.FinalFare ?? 0) })
                .ToList();

            ViewBag.RevenueLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
            ViewBag.RevenueValues = last7Days.Select(d => revenueData.FirstOrDefault(r => r.Date == d)?.Total ?? 0).ToList();

            var usageData = _context.Rentals
                .Include(r => r.Vehicle)
                .GroupBy(r => r.Vehicle.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();
            ViewBag.UsageLabels = usageData.Select(x => x.Type).ToList();
            ViewBag.UsageCounts = usageData.Select(x => x.Count).ToList();

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