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

        List<decimal> GetTotalPrice(DateTime[] intervals)
        {
            using (var context = new RentalManagementPlatformSqlContext())
            {
                var results = new List<decimal>();

                for (var i = 0; i < intervals.Length - 1; i++)
                {
                    var start = intervals[i];
                    var end = intervals[i + 1];

                    // 避免 Sum() 在空集合拋出例外，改用 (decimal?) + ?? 0m
                    var totalPrice = context.Bookings
                        .Where(x => x.CreatedAt != null &&
                                    x.CreatedAt.Value >= start &&
                                    x.CreatedAt.Value < end &&
                                    x.TotalPrice != null)
                        .Sum(x => (decimal?)x.TotalPrice) ?? 0m;

                    results.Add(totalPrice);
                }

                return results;
            }
        }

        [HttpPost]
        public IActionResult GetChartData(string timeUnit, DateTime start, DateTime end)
        {
            var intervals = GetIntervals(timeUnit, start, end).ToArray();

            var labels = intervals.Select(x => timeUnit switch
            {
                "day" => x.ToString("yyyy/M/d"),
                "week" => x.ToString("yyyy/M/d"),
                "month" => x.ToString("yyyy/M"),
                "quarter" => $"{x:yyyy}/Q{((x.Month - 1) / 3 + 1)}",
                "year" => x.ToString("yyyy"),
                _ => throw new ArgumentException("Invalid timeUnit")
            }).SkipLast(1).ToArray();

            var data = GetTotalPrice(intervals);

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
