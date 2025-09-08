using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.ReportForm.Helpers;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public class ReportFormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetChartData(string timeUnit, DateTime? start, DateTime? end)
        {
            var labels = new[] { "一月", "二月", "三月", "四月", "五月" , "一月", "二月", "三月", "四月", "五月" , "一月", "二月", "三月", "四月", "五月" };
            var data = new[] { 12000, 19000, 3000, 5000, 2000 , 12000, 19000, 3000, 5000, 2000 , 12000, 19000, 3000, 5000, 2000 };
            var dataCount = data.Length;
            var colors = ColorPaletteHelper.GenerateColors(dataCount);

            return Json(new
            {
                labels,
                data,
                colors,
                label = "銷售額 (NTD)"
            });
        }
    }
}
