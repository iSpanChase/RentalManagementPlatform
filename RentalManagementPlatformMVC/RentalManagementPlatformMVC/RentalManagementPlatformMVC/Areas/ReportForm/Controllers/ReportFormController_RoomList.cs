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
            (DateTime[] intervals, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var results = new List<decimal>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<RoomList> rooms = RoomSelecter(cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
                rooms = RoomActiveSelecter(intervals, i, rooms);
                // 避免 Sum() 在空集合拋出例外，改用 (decimal?) + ?? 0m
                var averagePrice = await rooms.AverageAsync(x => (decimal?)x.PricePerNight) ?? 0m;
                results.Add(averagePrice);
            }
            return results;
        }

        async Task<List<decimal>> RoomListAverageRating
            (DateTime[] intervals, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var results = new List<decimal>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                var start = intervals[i];
                var end = intervals[i + 1];

                IQueryable<RoomList> rooms = RoomSelecter(cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
                //rooms = RoomActiveSelecter(intervals, i, rooms);

                var avg = await (
                    from rv in _context.Reviews
                    join rl in rooms on rv.RoomId equals rl.RoomId
                    where rv.CreatedAt >= start && rv.CreatedAt < end
                    select (decimal?)rv.Rating
                ).AverageAsync();

                results.Add(avg ?? 0m);
            }
            return results;
        }

        async Task<List<int>> RoomListCount
            (DateTime[] intervals, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var results = new List<int>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<RoomList> rooms = RoomSelecter(cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
                rooms = RoomActiveSelecter(intervals, i, rooms);
                var averageRating = await rooms.CountAsync();
                results.Add(averageRating);
            }
            return results;
        }

        async Task<List<int>> RoomListCreateCount
            (DateTime[] intervals, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var results = new List<int>();
            for (var i = 0; i < intervals.Length - 1; i++)
            {
                IQueryable<RoomList> rooms = RoomSelecter(cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
                rooms = RoomCreateSelecter(intervals, i, rooms);
                var averageRating = await rooms.CountAsync();
                results.Add(averageRating);
            }
            return results;
        }


        private static IQueryable<RoomList> RoomActiveSelecter
            (DateTime[] intervals, int i, IQueryable<RoomList> rooms)
        {
            rooms = rooms.Where(x => x.CreatedAt != null && x.CreatedAt.Value < intervals[i + 1]);
            return rooms;
        }

        private static IQueryable<RoomList> RoomCreateSelecter
            (DateTime[] intervals, int i, IQueryable<RoomList> rooms)
        {
            rooms = rooms.Where(x => x.CreatedAt != null &&
                                                            x.CreatedAt.Value >= intervals[i] &&
                                                            x.CreatedAt.Value < intervals[i + 1]);
            return rooms;
        }

        IQueryable<RoomList> RoomSelecter
            (int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var result = _context.RoomLists
                .Where(x => priceMin == null || priceMin.Value <= x.PricePerNight)
                .Where(x => priceMax == null || priceMax.Value > x.PricePerNight)
                .Join(_context.Addresses, rl => rl.AddressId, a => a.AddressId, (rl, a) => new { rl, a })
                .Join(_context.Districts, rla => rla.a.DistrictId, d => d.DistrictId, (rla, d) => new { rla, d })
                .Join(_context.Cities, rlad => rlad.d.CityId, c => c.CityId, (rlad, c) => new { rlad, c })
                .Where(x => districtId == null || x.rlad.d.DistrictId == districtId)
                .Where(x => cityId == null || x.c.CityId == cityId)
                .Select(x => x.rlad.rla.rl)
                .GroupJoin(_context.Reviews, rl => rl.RoomId, rv => rv.RoomId, (rl, reviews) => new { rl, reviews })
                .Where(x => ratingMin == null || ratingMax == null || x.reviews.Any())
                .Where(x => ratingMin == null || ratingMin.Value <= x.reviews.Average(r => (double?)r.Rating))
                .Where(x => ratingMax == null || ratingMax.Value > x.reviews.Average(r => (double?)r.Rating))
                .Select(x => x.rl);

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> RoomListAveragePriceTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await RoomListAveragePrice(intervals.ToArray(), cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "房源每晚平均金額 (NTD)"
            });
        }

        [HttpPost]
        public async Task<IActionResult> RoomListAverageRatingTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await RoomListAverageRating(intervals.ToArray(), cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "房源平均評分"
            });
        }

        [HttpPost]
        public async Task<IActionResult> RoomListCountTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await RoomListCount(intervals.ToArray(), cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "房源數量"
            });
        }

        [HttpPost]
        public async Task<IActionResult> RoomListCreateCountTrend
            (string timeUnit, DateTime start, DateTime end, int? cityId, int? districtId, int? priceMin, int? priceMax, int? ratingMin, int? ratingMax)
        {
            var intervals = GetIntervals(timeUnit, start, end);
            var labels = FormatDateIntervals(timeUnit, intervals).ToArray();
            var data = await RoomListCreateCount(intervals.ToArray(), cityId, districtId, priceMin, priceMax, ratingMin, ratingMax);
            var colors = ColorPaletteHelper.GenerateColors(labels.Length);
            return Json(new
            {
                labels,
                data,
                colors,
                label = "房源創建數量"
            });
        }

    }
}
