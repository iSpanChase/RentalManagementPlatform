using Microsoft.AspNetCore.Mvc;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
