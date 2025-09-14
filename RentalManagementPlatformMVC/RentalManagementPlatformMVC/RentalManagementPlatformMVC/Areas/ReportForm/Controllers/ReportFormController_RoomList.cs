using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.ReportForm.Helpers;
using RentalManagementPlatformMVC.Areas.ReportForm.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    public partial class ReportFormController : Controller
    {
        async Task<List<decimal>> RoomListAveragePrice
            (DateTime[] intervals, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax, string status)
        {
            
            var results = new List<decimal>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<RoomList> bookingWhereAddress = RoomSelecter(cityId, districtId, priceMin, priceMax, ratingMin, ratingMax, status);
                bookingWhereAddress = RoomActiveSelecter(intervals, i, bookingWhereAddress);
                // 避免 Sum() 在空集合拋出例外，改用 (decimal?) + ?? 0m
                var averagePrice = await bookingWhereAddress.AverageAsync(x => (decimal?)x.PricePerNight) ?? 0m;
                results.Add(averagePrice);
            }
            return results;
        }

        private static IQueryable<RoomList> RoomActiveSelecter
            (DateTime[] intervals, int i, IQueryable<RoomList> bookingWhereAddress)
        {
            bookingWhereAddress = bookingWhereAddress
                .Where(x => x.CreatedAt != null && x.CreatedAt.Value < intervals[i + 1]);
            return bookingWhereAddress;
        }

        IQueryable<RoomList> RoomSelecter
            (int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax, string status)
        {
            var result = _context.RoomLists
                .Where(x => priceMin == null || priceMin.Value <= x.PricePerNight)
                .Where(x => priceMax == null || priceMax.Value > x.PricePerNight)
                .Join(_context.Addresses, rl => rl.AddressId, a => a.AddressId, (rl, a) => new { rl, a })
                .Join(_context.Districts, rla => rla.a.DistrictId, d => d.DistrictId, (rla, d) => new { rla, d })
                .Join(_context.Cities, rlad => rlad.d.CityId, c => c.CityId, (rlad, c) => new { rlad, c })
                .Where(x => districtId == null || x.rlad.d.DistrictId == districtId)
                .Where(x => cityId == null || x.c.CityId == cityId)
                .Where(x => status == null || (!string.IsNullOrEmpty(x.rlad.rla.rl.Status) && status.Contains(x.rlad.rla.rl.Status)))
                .Select(x => x.rlad.rla.rl)
                .GroupJoin(_context.Reviews, rl => rl.RoomId, rv => rv.RoomId, (rl, reviews) => new { rl, reviews })
                .Where(x => x.reviews.Any())
                .Where(x => ratingMin == null || ratingMin.Value <= x.reviews.Average(r => (double?)r.Rating))
                .Where(x => ratingMax == null || ratingMax.Value > x.reviews.Average(r => (double?)r.Rating))
                .Select(x => x.rl);

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> RoomListAveragePriceTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax, string status)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await RoomListAveragePrice(intervals.ToArray(), cityId, districtId, priceMin, priceMax, ratingMin, ratingMax, status);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "房源每晚平均金額 (NTD)"
            });
        }

    }
}
