using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;
using SaigonRide.Models.Entities;

namespace SaigonRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (string.IsNullOrEmpty(user.FullName) || string.IsNullOrEmpty(user.Email))
            {
                ModelState.AddModelError(string.Empty, "Name and Email cannot be empty.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Index", "Station");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == "admin@tdtu.edu.vn" && password == "admin123")
            {
                return RedirectToAction("Index", "Station");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }
    }
}