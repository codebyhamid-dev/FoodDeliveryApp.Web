using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admin role can access
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Products
        public IActionResult Products()
        {
            // If you want the AdminId from the claims (optional):
            // var adminIdClaim = User.FindFirst("AdminId");
            // int adminId = adminIdClaim != null ? int.Parse(adminIdClaim.Value) : 0;

            // Fetch items (AsNoTracking for read-only)
            var items = _db.Items
                .AsNoTracking()
                .ToList();

            return View(items);
        }

        // GET: /Admin/Orders
        public IActionResult Orders()
        {
            // If you need the admin ID:
            // var adminIdClaim = User.FindFirst("AdminId");
            // int adminId = adminIdClaim != null ? int.Parse(adminIdClaim.Value) : 0;

            // AutoRejectOldPendingOrders();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Fetch only today's orders AsNoTracking
            var ordersToday = _db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
                .Where(o => o.OrderDateTime >= today && o.OrderDateTime < tomorrow)
                .OrderByDescending(o => o.OrderDateTime)
                .ToList();

            var pendingOrders = ordersToday.Where(o => o.Status == OrderStatus.Pending);
            var acceptedOrders = ordersToday.Where(o => o.Status == OrderStatus.Accepted);
            var rejectedOrders = ordersToday.Where(o => o.Status == OrderStatus.Rejected);

            // Convert to ViewModels
            ViewBag.PendingOrders = MapToAdminOrdersViewModel(pendingOrders);
            ViewBag.AcceptedOrders = MapToAdminOrdersViewModel(acceptedOrders);
            ViewBag.RejectedOrders = MapToAdminOrdersViewModel(rejectedOrders);

            return View();
        }

        // POST: /Admin/AcceptOrder
        [HttpPost]
        public IActionResult AcceptOrder(int orderId)
        {
            // No more session checks - [Authorize(Roles="Admin")] ensures only admins can reach here
            var order = _db.Orders.Find(orderId);
            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Accepted;
                _db.SaveChanges();
            }
            return RedirectToAction("Orders");
        }

        // POST: /Admin/RejectOrder
        [HttpPost]
        public IActionResult RejectOrder(int orderId)
        {
            var order = _db.Orders.Find(orderId);
            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Rejected;
                _db.SaveChanges();
            }
            return RedirectToAction("Orders");
        }

        private void AutoRejectOldPendingOrders()
        {
            var fiveMinutesAgo = DateTime.Now.AddMinutes(-5);

            var oldPendingOrders = _db.Orders
                .Where(o => o.Status == OrderStatus.Pending && o.OrderDateTime < fiveMinutesAgo)
                .ToList();

            foreach (var ord in oldPendingOrders)
            {
                ord.Status = OrderStatus.Rejected;
            }
            _db.SaveChanges();
        }

        // Maps Orders to AdminOrdersViewModel
        private List<AdminOrdersViewModel> MapToAdminOrdersViewModel(IEnumerable<Order> orders)
        {
            var result = new List<AdminOrdersViewModel>();
            foreach (var o in orders)
            {
                var itemsList = o.OrderItems.Select(oi =>
                    $"{oi.Item.ItemName} x{oi.Quantity} (${oi.UnitPrice})").ToList();

                result.Add(new AdminOrdersViewModel
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer.CustomerName,
                    CustomerEmail = o.Customer.Email,
                    Items = itemsList,
                    TotalPrice = o.Price,
                    OrderDateTime = o.OrderDateTime.ToString("g")
                });
            }
            return result;
        }
    }
}