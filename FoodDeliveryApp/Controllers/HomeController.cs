using Microsoft.AspNetCore.Mvc;
namespace FoodDeliveryApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}