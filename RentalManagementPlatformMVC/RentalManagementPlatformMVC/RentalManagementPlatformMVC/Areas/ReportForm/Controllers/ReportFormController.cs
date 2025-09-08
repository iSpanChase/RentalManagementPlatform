using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.ReportForm.Helpers;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public class ReportFormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        IEnumerable<DateTime> GetIntervals(string timeUnit, DateTime start, DateTime end)
        {
            DateTime temp = start;
            while (temp <= end)
            {
                yield return temp;
                temp = timeUnit switch
                {
                    "day" => temp.AddDays(1),
                    "week" => temp.AddDays(7),
                    "month" => temp.AddMonths(1),
                    "quarter" => temp.AddMonths(3),
                    "year" => temp.AddYears(1),
                    _ => throw new ArgumentException("Invalid timeUnit")
                };
            }
            // 額外補一個結束點
            yield return temp;
        }
        [HttpPost]
        public IActionResult GetChartData(string timeUnit, DateTime start, DateTime end)
        {
            var intervals = GetIntervals(timeUnit, start, end).ToArray();

            using (var context = new RentalManagementPlatformSqlContext())
            {
                // 先抓出範圍內的所有資料
                var bookings = context.Bookings
                    .Where(x => x.CreatedAt != null &&
                                x.CreatedAt >= start &&
                                x.CreatedAt <= end &&
                                x.TotalPrice != null)
                    .ToList();

                var data = intervals.Zip(intervals.Skip(1), (s, e) =>
                    bookings.Where(x => x.CreatedAt != null && x.CreatedAt.Value >= s && x.CreatedAt.Value < e)
                            .Sum(x => x.TotalPrice ?? 0)
                ).ToArray();

                var labels = intervals.Select(x => timeUnit switch
                {
                    "day" => x.ToString("yyyy/M/d"),
                    "week" => x.ToString("yyyy/M/d"),
                    "month" => x.ToString("yyyy/M"),
                    "quarter" => $"{x:yyyy}/Q{((x.Month - 1) / 3 + 1)}",
                    "year" => x.ToString("yyyy"),
                    _ => throw new ArgumentException("Invalid timeUnit")
                }).SkipLast(1).ToArray();

                var colors = ColorPaletteHelper.GenerateColors(labels.Length);

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
}
