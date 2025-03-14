using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDeliveryApp.Controllers
{
    public class CustomerAccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerAccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /CustomerAccount/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /CustomerAccount/Register
        [HttpPost]
        public IActionResult Register(CustomerRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return the same view with validation errors
                return View(model);
            }

            // Check if email exists
            bool emailExists = _db.Customers
                .AsNoTracking()
                .Any(c => c.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError("", "Email already in use.");
                return View(model);
            }

            // Create a new Customer entity from the ViewModel
            var newCustomer = new Customer
            {
                CustomerName = model.CustomerName,
                Email = model.Email,
                Password = model.Password, // In production, store a hashed password!
                PhoneNumber = model.PhoneNumber,
                Address = model.Address
            };

            _db.Customers.Add(newCustomer);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: /CustomerAccount/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /CustomerAccount/Login
        [HttpPost]
        public async Task<IActionResult> Login(CustomerLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var customer = _db.Customers
                .FirstOrDefault(c => c.Email == model.Email && c.Password == model.Password);

            if (customer == null)
            {
                ModelState.AddModelError("", "Invalid Customer Credentials.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, customer.CustomerName),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim("CustomerId", customer.CustomerId.ToString()),
                // Optionally set a role for customers
                new Claim(ClaimTypes.Role, "Customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("MyOrders", "Customer");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        } 
    }
}