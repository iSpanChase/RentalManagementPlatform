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

        [HttpPost]
        public IActionResult GetChartData(string type, string timeUnit, DateTime? start, DateTime? end)
        {
            // 模擬假資料，實際應從 DB 篩選
            var labels = new[] { "一月", "二月", "三月", "四月", "五月" };
            var data = new[] { 12000, 19000, 3000, 5000, 2000 };
            var colors = new[]
            {
            "rgba(255, 99, 132, 0.5)",
            "rgba(54, 162, 235, 0.5)",
            "rgba(255, 206, 86, 0.5)",
            "rgba(75, 192, 192, 0.5)",
            "rgba(153, 102, 255, 0.5)"
        };

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
