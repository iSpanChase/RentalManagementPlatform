using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.ReportForm.Helpers;
using RentalManagementPlatformMVC.Areas.ReportForm.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public partial class ReportFormController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public ReportFormController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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

        IEnumerable<string> FormatDateIntervals(string timeUnit, IEnumerable<DateTime> intervals)
        {
            return intervals.Select(x => timeUnit switch
            {
                "day" => x.ToString("yyyy/M/d"),
                "week" => x.ToString("yyyy/M/d"),
                "month" => x.ToString("yyyy/M"),
                "quarter" => $"{x:yyyy}/Q{((x.Month - 1) / 3 + 1)}",
                "year" => x.ToString("yyyy"),
                _ => throw new ArgumentException("Invalid timeUnit")
            }).SkipLast(1);
        }

        [HttpGet] //Get : ReportForm/ReportForm/Cities
        public async Task<IActionResult> Cities()
        {
            var citiesDataForm = await _context.Cities.Select(x => new CityVM(x)).ToArrayAsync();
            return Json(citiesDataForm);
        }

        [HttpGet]
        public async Task<IActionResult> Districts(int cityId)
        {
            var DistrictsDataForm = await _context.Districts.Where(x => x.CityId == cityId).Select(x => new DistrictVM(x)).ToArrayAsync();
            return Json(DistrictsDataForm);
        }

        [HttpPost]
        public async Task<IActionResult> Roles()
        {
            var RolesDataForm = await _context.Roles.Select(x => new RoleVM(x)).ToArrayAsync();
            return Json(RolesDataForm);
        }
    }
}
