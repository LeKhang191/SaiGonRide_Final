using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SaigonRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context) => _context = context;

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (string.IsNullOrEmpty(user.FullName) || string.IsNullOrEmpty(user.Email))
            {
                ModelState.AddModelError(string.Empty, "Please fill in your name and email.");
                return View(user);
            }

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error saving to DB: " + ex.Message);
                return View(user);
            }
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == "admin@ex.com" && password == "admin123")
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                return RedirectToAction("Index", "Station");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserRole", user.UserType);
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        [HttpGet]
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            bool hasActiveTrip = await _context.Rentals
                .AnyAsync(r => r.UserId == id && r.Status == "Active");

            if (hasActiveTrip)
            {
                TempData["Error"] = "Cannot delete: This user currently has an active rental!";
                return RedirectToAction("Index");
            }

            try
            {
                var userRentals = _context.Rentals.Where(r => r.UserId == id);
                _context.Rentals.RemoveRange(userRentals);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearUserHistory(int id)
        {
            var userRentals = _context.Rentals.Where(r => r.UserId == id).ToList();

            if (userRentals.Any())
            {
                _context.Rentals.RemoveRange(userRentals);
                await _context.SaveChangesAsync();
                TempData["Success"] = "This user's car rental history has been completely deleted..";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";

            return RedirectToAction("Login", "Account");
        }
    }
}