using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CustomerController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: /Customer/PlaceOrder
        public IActionResult PlaceOrder()
        {
            //Manage Authentication
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }
            var items = _db.Items
                .AsNoTracking()
                .Where(i => i.IsAvailable)
                .ToList();

            // 3) Map items to the new view model
            var model = new MultipleOrderItemViewModel
            {
                Items = items.Select(i => new OrderItemViewModel
                {
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    Price = i.Price,
                    IsAvailable = i.IsAvailable,
                    Quantity = 0
                }).ToList()
            };
            return View(model);
        }
        // POST: /Customer/PlaceOrder
        // Processes the selected items and creates a single Order with multiple OrderItems
        [HttpPost]
        public IActionResult PlaceOrder(MultipleOrderItemViewModel model)
        {
            // 1) Verify the user is still authenticated
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }
            int customerId = int.Parse(customerIdClaim.Value);
            // 2) Filter out items with Quantity > 0
            var selectedItems = model.Items.Where(x => x.Quantity > 0).ToList();
            if (!selectedItems.Any())
            {
                ModelState.AddModelError("", "No items selected. Please pick at least one item.");
                return View(model); // Re-display the form
            }
            // 3) Create a new order
            var order = new Order
            {
                CustomerId = customerId,
                OrderDateTime = DateTime.Now,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>(),
            };
            // 4) For each selected item, create an OrderItem
            foreach (var sel in selectedItems)
            {
                // 1) Look up the real item from the DB
                var dbItem = _db.Items.FirstOrDefault(i => i.ItemId == sel.ItemId && i.IsAvailable);
                if (dbItem == null)
                {
                    // The item was not found or not available
                    continue; // or handle error
                }
                // 2) Use the real, current price from DB
                var orderItem = new OrderItem
                {
                    ItemId = dbItem.ItemId,
                    Quantity = sel.Quantity,
                    UnitPrice = dbItem.Price,
                };
                order.OrderItems.Add(orderItem);
            }
            // 5) Calculate total Price
            order.Price = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
            // 6) Save
            _db.Orders.Add(order);
            _db.SaveChanges();
            // 7) Redirect to MyOrders
            return RedirectToAction("MyOrders");
        }

        //Fetch Order


        public IActionResult MyOrders()
        {
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }
            int customerId = int.Parse(customerIdClaim.Value);
            // Get all orders for this customer
            var orders = _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDateTime)
                .ToList();

            var customerOrders = new List<CustomerOrderViewModel>();
            foreach (var order in orders)
            {
                var orderVm = new CustomerOrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderTotalPrice = order.Price,
                    Status = order.Status.ToString(),
                    OrderDateTime = order.OrderDateTime.ToString("g"),
                    Items = new List<CustomerOrderItemViewModel>()
                };
                // Add each item in the order
                foreach (var oi in order.OrderItems)
                {
                    orderVm.Items.Add(new CustomerOrderItemViewModel
                    {
                        ItemName = oi.Item.ItemName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    });
                }
                customerOrders.Add(orderVm);
            }
            return View(customerOrders);
        }
    }
}
