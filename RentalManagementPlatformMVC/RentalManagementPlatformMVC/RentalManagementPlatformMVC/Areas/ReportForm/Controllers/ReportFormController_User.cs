using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.ReportForm.Helpers;
using RentalManagementPlatformMVC.Areas.ReportForm.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    public partial class ReportFormController : Controller
    {
        async Task<List<decimal>> UserExistCount
            (DateTime[] intervals, string gender, int? ageMin, int? ageMax, string roleId)
        {
            var results = new List<decimal>();
            IQueryable<Models.User> UserSelected = UserSelecter(gender, ageMin, ageMax, roleId);
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                var count = await UserSelected
                    .Where(x => x.CreatedAt != null && x.CreatedAt.Value < intervals[i + 1])
                    .CountAsync();
                results.Add(count);
            }
            return results;
        }
        async Task<List<decimal>> UserCreateCount
            (DateTime[] intervals, string gender, int? ageMin, int? ageMax, string roleId)
        {
            var results = new List<decimal>();
            IQueryable<Models.User> UserSelected = UserSelecter(gender, ageMin, ageMax, roleId);
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                var count = await UserSelected
                    .Where(x => x.CreatedAt != null &&
                        x.CreatedAt.Value >= intervals[i] &&
                        x.CreatedAt.Value < intervals[i + 1])
                    .CountAsync();
                results.Add(count);
            }
            return results;
        }
        IQueryable<Models.User> UserSelecter
            (string gender, int? ageMin, int? ageMax, string roleId)
        {
            var result = _context.Users
                .Where(x => gender == null || x.Gender == gender)
                .Where(x => ageMin == null || ageMin <= (((DateTime.Today.Year * 100 + DateTime.Today.Month) * 100 + DateTime.Today.Day - (x.BirthDate.Year * 100 + x.BirthDate.Month) * 100 - x.BirthDate.Day) / 10000))
                .Where(x => ageMax == null || ageMax > (((DateTime.Today.Year * 100 + DateTime.Today.Month) * 100 + DateTime.Today.Day - (x.BirthDate.Year * 100 + x.BirthDate.Month) * 100 - x.BirthDate.Day) / 10000))
                .Join(_context.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u, ur })
                .Where(x => roleId == null || roleId.Contains(x.ur.RoleId.ToString()))
                .Select(x => x.u)
                .Distinct();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> UserCountTrend
            (string timeUnit, DateTime start, DateTime end, string gender, int? ageMin, int? ageMax, string roleId)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await UserExistCount(intervals.ToArray(), gender, ageMin, ageMax, roleId);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "使用者數"
            });
        }

        [HttpPost]
        public async Task<IActionResult> UserCreateCountTrend
            (string timeUnit, DateTime start, DateTime end, string gender, int? ageMin, int? ageMax, string roleId)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await UserCreateCount(intervals.ToArray(), gender, ageMin, ageMax, roleId);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "使用者創建數"
            });
        }

        [HttpPost]
        public async Task<IActionResult> UserRoleComposition(string gender, int? ageMin, int? ageMax, string roleId)
        {
            var query = _context.Roles
                .Where(r => roleId == null || roleId.Contains(r.RoleId.ToString()))
                .Select(r => new {
                    r.RoleName,
                    Count = r.UserRoles.Count()
                });

            var labels = await query.Select(x => x.RoleName).ToArrayAsync();
            var data = await query.Select(x => x.Count).ToArrayAsync();
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "使用者數"
            });
        }
    }
}
