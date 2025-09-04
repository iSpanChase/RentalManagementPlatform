using Microsoft.AspNetCore.Mvc;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public class ReportFormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
