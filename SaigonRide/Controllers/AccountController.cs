using Microsoft.AspNetCore.Mvc;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using SaigonRide.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SaigonRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (string.IsNullOrEmpty(user.FullName) || string.IsNullOrEmpty(user.Email))
            {
                ModelState.AddModelError(string.Empty, "Please fill in your name and email.");
                return View(user);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError(string.Empty, "Email này đã được sử dụng.");
                return View(user);
            }

            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("PendingOtp", otp);
            HttpContext.Session.SetString("OtpExpiry", DateTime.UtcNow.AddMinutes(5).ToString("O"));
            HttpContext.Session.SetString("PendingUser_Name", user.FullName);
            HttpContext.Session.SetString("PendingUser_Email", user.Email);
            HttpContext.Session.SetString("PendingUser_Password", user.Password ?? "");
            HttpContext.Session.SetString("PendingUser_Type", user.UserType ?? "Local Tourist");
            HttpContext.Session.SetString("PendingUser_IdNumber", user.IdNumber ?? "");

            try
            {
                await _emailService.SendOtpAsync(user.Email, otp);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Không thể gửi email. Vui lòng kiểm tra lại địa chỉ email: " + ex.Message);
                return View(user);
            }

            TempData["PendingEmail"] = user.Email;
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            ViewBag.Email = TempData["PendingEmail"] as string
                ?? HttpContext.Session.GetString("PendingUser_Email");
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otpCode)
        {
            var storedOtp = HttpContext.Session.GetString("PendingOtp");
            var expiryStr = HttpContext.Session.GetString("OtpExpiry");

            if (storedOtp == null || expiryStr == null)
            {
                ModelState.AddModelError("", "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.");
                return View();
            }

            if (DateTime.Parse(expiryStr, null, System.Globalization.DateTimeStyles.RoundtripKind) < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn. Vui lòng đăng ký lại.");
                return View();
            }

            if (otpCode != storedOtp)
            {
                ModelState.AddModelError("", "Mã OTP không đúng. Vui lòng thử lại.");
                ViewBag.Email = HttpContext.Session.GetString("PendingUser_Email");
                return View();
            }

            var user = new User
            {
                FullName = HttpContext.Session.GetString("PendingUser_Name")!,
                Email = HttpContext.Session.GetString("PendingUser_Email")!,
                Password = HttpContext.Session.GetString("PendingUser_Password")!,
                UserType = HttpContext.Session.GetString("PendingUser_Type"),
                IdNumber = HttpContext.Session.GetString("PendingUser_IdNumber"),
            };

            HttpContext.Session.Remove("PendingOtp");
            HttpContext.Session.Remove("OtpExpiry");
            HttpContext.Session.Remove("PendingUser_Name");
            HttpContext.Session.Remove("PendingUser_Email");
            HttpContext.Session.Remove("PendingUser_Password");
            HttpContext.Session.Remove("PendingUser_Type");
            HttpContext.Session.Remove("PendingUser_IdNumber");

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi lưu tài khoản: " + ex.Message);
                return View();
            }

            HttpContext.Session.SetString("UserRole", user.UserType ?? "User");
            HttpContext.Session.SetInt32("UserId", user.UserId);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == "admin@ex.com" && password == "admin123")
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserRole", user.UserType ?? "User");
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
