using Microsoft.AspNetCore.Mvc;

namespace RentalManagementPlatformMVC.Areas.Booking.Controllers
{
    public class HomeController : Controller
    {
        [Area("Booking")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
