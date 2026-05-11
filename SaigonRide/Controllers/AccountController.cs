using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using System.Linq;

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
            if (email == "admin@tdtu.edu.vn" && password == "admin123")
                return RedirectToAction("Index", "Station");

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null) return RedirectToAction("Index", "Home");

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

            var userRentals = _context.Rentals.Where(r => r.UserId == id);
            _context.Rentals.RemoveRange(userRentals);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

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