using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.ReportForm.Helpers;
using RentalManagementPlatformMVC.Areas.ReportForm.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    [Area("ReportForm")]
    public class ReportFormController : Controller
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public ReportFormController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }
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

        List<decimal> BookingRevenueSum(DateTime[] intervals, int? cityId, int? districtId, string status)
        {
            var results = new List<decimal>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<Models.Booking> bookingWhereAddress = BookingSelecter(intervals, i, cityId, districtId, status);
                // 避免 Sum() 在空集合拋出例外，改用 (decimal?) + ?? 0m
                var totalPrice = bookingWhereAddress.Sum(x => (decimal?)x.TotalPrice) ?? 0m;
                results.Add(totalPrice);
            }
            return results;
        }
        List<int> BookingCount(DateTime[] intervals, int? cityId, int? districtId, string status)
        {
            var results = new List<int>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<Models.Booking> bookingWhereAddress = BookingSelecter(intervals, i, cityId, districtId, status);
                // 避免 Sum() 在空集合拋出例外，改用 (decimal?) + ?? 0m
                var count = bookingWhereAddress.Count();
                results.Add(count);
            }
            return results;
        }
        private IQueryable<Models.Booking> BookingSelecter
            (DateTime[] intervals, int i, int? cityId, int? districtId, string status)
        {
            var start = intervals[i];
            var end = intervals[i + 1];

            var result = _context.Bookings
                .Where(x => x.CreatedAt != null &&
                            x.CreatedAt.Value >= start &&
                            x.CreatedAt.Value < end &&
                            x.TotalPrice != null)
                .Join(_context.RoomLists, b => b.RoomId, rl => rl.RoomId, (b, rl) => new { b, rl })
                .Join(_context.Addresses, brl => brl.rl.AddressId, a => a.AddressId, (brl, a) => new { brl, a })
                .Join(_context.Districts, brla => brla.a.DistrictId, d => d.DistrictId, (brla, d) => new { brla, d })
                .Join(_context.Cities, brlad => brlad.d.CityId, c => c.CityId, (brlad, c) => new { brlad, c })
                .Where(x => districtId == null || x.brlad.d.DistrictId == districtId)
                .Where(x => cityId == null || x.c.CityId == cityId)
                .Where(x=> status==null || status.Contains(x.brlad.brla.brl.b.Status))
                .Select(x => x.brlad.brla.brl.b);
            return result;
        }

        [HttpPost]
        public IActionResult BookingRevenueTrend(string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId, string status)
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

            var data = BookingRevenueSum(intervals,cityId,districtId, status);

            var colors = ColorPaletteHelper.GenerateColors(labels.Length);

            return Json(new
            {
                labels,
                data,
                colors,
                label = "銷售額 (NTD)"
            });
        }

        [HttpPost]
        public IActionResult BookingCountTrend(string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId, string status)
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

            var data = BookingCount(intervals, cityId, districtId, status);

            var colors = ColorPaletteHelper.GenerateColors(labels.Length);

            return Json(new
            {
                labels,
                data,
                colors,
                label = "訂單數"
            });
        }

        List<decimal> UserExistCount(DateTime[] intervals, string gender, int? ageMin, int? ageMax, string roleId)
        {
            var results = new List<decimal>();
            IQueryable<Models.User> UserSelected = UserSelecter(gender, ageMin, ageMax, roleId);
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                var count = UserSelected
                    .Where(x => x.CreatedAt != null && x.CreatedAt.Value < intervals[i + 1])
                    .Count();
                results.Add(count);
            }
            return results;
        }
        private IQueryable<Models.User> UserSelecter
            (string gender, int? ageMin, int? ageMax, string roleId)
        {
            var result = _context.Users
                .Where(x => gender == null || x.Gender == gender)
                .Where(x => ageMin == null || ageMin <= (((DateTime.Today.Year * 100 + DateTime.Today.Month) * 100 + DateTime.Today.Day - (x.BirthDate.Year * 100 + x.BirthDate.Month) * 100 - x.BirthDate.Day) / 10000))
                .Where(x => ageMax == null || ageMin > (((DateTime.Today.Year * 100 + DateTime.Today.Month) * 100 + DateTime.Today.Day - (x.BirthDate.Year * 100 + x.BirthDate.Month) * 100 - x.BirthDate.Day) / 10000))
                .Join(_context.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u, ur })
                .Where(x => roleId == null || roleId.Contains(x.ur.RoleId.ToString()))
                .Select(x => x.u)
                .Distinct();
            return result;
        }
        [HttpPost]
        public IActionResult UserCountTrend(string timeUnit, DateTime start, DateTime end, string gender, int? ageMin, int? ageMax, string roleId)
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

            var data = UserExistCount(intervals,gender,ageMin,ageMax,roleId);

            var colors = ColorPaletteHelper.GenerateColors(labels.Length);

            return Json(new
            {
                labels,
                data,
                colors,
                label = "訂單數"
            });
        }

        [HttpGet] //Get : ReportForm/ReportForm/Cities
        public IActionResult Cities()
        {
            var citiesDataForm = _context.Cities.Select(x => new CityVM(x)).ToList();
            return Json(citiesDataForm);
        }

        [HttpGet]
        public IActionResult Districts(int cityId)
        {
            var DistrictsDataForm = _context.Districts.Where(x=>x.CityId ==cityId).Select(x => new DistrictVM(x)).ToList();
            return Json(DistrictsDataForm);
        }

        [HttpPost]
        public IActionResult Roles()
        {
            var RolesDataForm = _context.Roles.Select(x => new RoleVM(x)).ToList();
            return Json(RolesDataForm);
        }
    }
}
