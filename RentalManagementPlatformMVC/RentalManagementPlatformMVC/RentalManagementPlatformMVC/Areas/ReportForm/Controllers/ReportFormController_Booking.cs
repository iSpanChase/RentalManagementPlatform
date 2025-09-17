using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.ReportForm.Helpers;
using RentalManagementPlatformMVC.Areas.ReportForm.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    public partial class ReportFormController : Controller
    {
        async Task<List<decimal>> BookingAverageAmount
            (DateTime[] intervals, int? cityId, int? districtId)
        {
            var results = new List<decimal>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<Models.Booking> bookingWhereAddress = BookingSelecter(intervals, i, cityId, districtId);
                // 避免 Sum() 在空集合拋出例外，改用 (decimal?) + ?? 0m
                var totalPrice = await bookingWhereAddress.AverageAsync(x => (decimal?)x.TotalPrice) ?? 0m;
                results.Add(totalPrice);
            }
            return results;
        }
        async Task<List<int>> BookingCount(DateTime[] intervals, int? cityId, int? districtId)
        {
            var results = new List<int>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<Models.Booking> bookingWhereAddress = BookingSelecter(intervals, i, cityId, districtId);
                var count = await bookingWhereAddress.CountAsync();
                results.Add(count);
            }
            return results;
        }

        async Task<int> BookingCount(int? cityId, int? districtId)
        {
            IQueryable<Models.Booking> bookingWhereAddress = BookingSelecter(cityId, districtId);
            var count = await bookingWhereAddress.CountAsync();
            return count;
        }

        async Task<List<decimal>> BookingTotalAmount
            (DateTime[] intervals, int? cityId, int? districtId)
        {
            var results = new List<decimal>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<Models.Booking> bookingWhereAddress = BookingSelecter(intervals, i, cityId, districtId);
                // 避免 Sum() 在空集合拋出例外，改用 (decimal?) + ?? 0m
                var totalPrice = await bookingWhereAddress.SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;
                results.Add(totalPrice);
            }
            return results;
        }

        IQueryable<Models.Booking> BookingSelecter(DateTime[] intervals, int i, int? cityId, int? districtId)
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
                .Select(x => x.brlad.brla.brl.b);
            return result;
        }

        IQueryable<Models.Booking> BookingSelecter(int? cityId, int? districtId)
        {
            var result = _context.Bookings
                .Where(x => x.CreatedAt != null && x.TotalPrice != null)
                .Join(_context.RoomLists, b => b.RoomId, rl => rl.RoomId, (b, rl) => new { b, rl })
                .Join(_context.Addresses, brl => brl.rl.AddressId, a => a.AddressId, (brl, a) => new { brl, a })
                .Join(_context.Districts, brla => brla.a.DistrictId, d => d.DistrictId, (brla, d) => new { brla, d })
                .Join(_context.Cities, brlad => brlad.d.CityId, c => c.CityId, (brlad, c) => new { brlad, c })
                .Where(x => districtId == null || x.brlad.d.DistrictId == districtId)
                .Where(x => cityId == null || x.c.CityId == cityId)
                .Select(x => x.brlad.brla.brl.b);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> BookingAverageAmountTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await BookingAverageAmount(intervals.ToArray(), cityId, districtId);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "平均金額 (NTD)"
            });
        }

        [HttpPost]
        public async Task<IActionResult> BookingCountTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await BookingCount(intervals.ToArray(), cityId, districtId);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "訂單數"
            });
        }

        [HttpPost]
        public async Task<IActionResult> BookingTotalAmountTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await BookingTotalAmount(intervals.ToArray(), cityId, districtId);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "總金額 (NTD)"
            });
        }

        [HttpPost]
        public async Task<IActionResult> TotalBookings(int? cityId, int? districtId)
        {
            var data = await BookingCount(cityId, districtId);
            return Json(new { data });
        }
    }
}
