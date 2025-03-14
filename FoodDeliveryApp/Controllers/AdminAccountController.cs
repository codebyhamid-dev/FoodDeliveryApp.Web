using System.Security.Claims;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Controllers
{
    public class AdminAccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminAccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        // POST: /AdminAccount/Login
        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var admin = _db.Admins
                .FirstOrDefault(a => a.Email == model.Email && a.Password == model.Password);
            if (admin == null)
            {
                ModelState.AddModelError("", "Invalid Admin Credentials.");
                return View(model);
            }
            // Build admin claims (e.g., email, role, etc.)
            var Claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,admin.Name),
                new Claim (ClaimTypes.Email,admin.Email),
                new Claim("AdminId", admin.AdminId.ToString()),
                // Optionally, set role for role-based checks
                new Claim(ClaimTypes.Role, "Admin"),
            };
            var claimsIdentity=new ClaimsIdentity(Claims,CookieAuthenticationDefaults.AuthenticationScheme);
            var principal=new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);
            // Redirect to admin area
            return RedirectToAction("Orders", "Admin");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
