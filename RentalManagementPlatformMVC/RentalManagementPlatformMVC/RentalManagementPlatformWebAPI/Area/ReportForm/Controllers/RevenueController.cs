using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Report;
using RentalManagementPlatformWebAPI.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public RevenueController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        [HttpPost("GetRevenue")]
        public async Task<IActionResult> GetRevenue([FromBody] RevenueRequestDto req)
        {
            // TODO: 之後需從登入資訊取得 HostId
            int hostId = 47;

            List<int> roomIdsToQuery;

            // 如果前端沒有提供任何 RoomId，則預設查詢該 host 的所有 room
            if (req.RoomIds == null || !req.RoomIds.Any())
            {
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId)
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }
            else
            {
                // 否則，使用前端指定的 RoomId，但仍需驗證這些 RoomId 是否屬於該 host
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId && req.RoomIds.Contains(r.RoomId))
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }

            if (!roomIdsToQuery.Any())
            {
                return Ok(new object[0]);
            }

            // 根據 RoomId 和日期範圍篩選 Bookings
            // 同時僅納入已完成或已確認的訂單，排除取消或未確認的訂單
            var revenueQuery = _context.Bookings
                .Where(b => b.Status == "Completed" || b.Status == "Confirmed")
                .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                .Where(b => b.CheckIn.HasValue && b.CheckIn.Value.Date >= req.StartDate.Date && b.CheckIn.Value.Date <= req.EndDate.Date);


            switch (req.GroupBy?.ToLower())
            {
                case "week":
                    // Helper to get the start of the week (Monday)
                    static DateTime GetStartOfWeek(DateTime dt)
                    {
                        int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
                        return dt.AddDays(-1 * diff).Date;
                    }

                    // 1. Group by week and calculate revenue
                    var revenueByWeek = revenueQuery
                        .AsEnumerable()
                        .GroupBy(b => GetStartOfWeek(b.CheckIn.Value))
                        .Select(g => new
                        {
                            Date = g.Key,
                            Revenue = g.Sum(b => b.TotalPrice ?? 0)
                        })
                        .ToList();

                    var revenueDictWeek = revenueByWeek.ToDictionary(r => r.Date, r => r.Revenue);

                    // 2. Generate all weeks in the range
                    var allWeeksInRange = new List<DateTime>();
                    var loopDateWeek = GetStartOfWeek(req.StartDate);
                    var finalDateWeek = GetStartOfWeek(req.EndDate);

                    while (loopDateWeek <= finalDateWeek)
                    {
                        allWeeksInRange.Add(loopDateWeek);
                        loopDateWeek = loopDateWeek.AddDays(7);
                    }

                    // 3. Combine and fill zeros
                    var fullWeeklyReport = allWeeksInRange.Select(weekStart => new
                    {
                        date = weekStart.ToString("yyyy-MM-dd"),
                        revenue = revenueDictWeek.TryGetValue(weekStart, out var revenue) ? revenue : 0
                    });

                    return Ok(fullWeeklyReport.OrderBy(r => r.date));

                case "month":
                    // 1. 從資料庫取得有收入的月份資料
                    var revenueByMonth = await revenueQuery
                        .GroupBy(b => new { Year = b.CheckIn.Value.Year, Month = b.CheckIn.Value.Month })
                        .Select(g => new
                        {
                            Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                            Revenue = g.Sum(b => b.TotalPrice ?? 0)
                        })
                        .ToListAsync();

                    var revenueDict = revenueByMonth.ToDictionary(r => r.Date, r => r.Revenue);

                    // 2. 產生日期區間內的所有月份
                    var allMonthsInRange = new List<DateTime>();
                    var loopDate = new DateTime(req.StartDate.Year, req.StartDate.Month, 1);
                    // 確保涵蓋到結束日期的月份
                    var finalDate = new DateTime(req.EndDate.Year, req.EndDate.Month, 1);

                    while (loopDate <= finalDate)
                    {
                        allMonthsInRange.Add(loopDate);
                        loopDate = loopDate.AddMonths(1);
                    }

                    // 3. 合併資料，沒有收入的月份補 0
                    var fullRevenueReport = allMonthsInRange.Select(month => new
                    {
                        date = month.ToString("yyyy-MM-dd"),
                        revenue = revenueDict.TryGetValue(month, out var revenue) ? revenue : 0
                    });

                    return Ok(fullRevenueReport.OrderBy(r => r.date));

                case "day":
                default:
                    // 1. 從資料庫取得有收入的每日資料
                    var revenueByDay = await revenueQuery
                        .GroupBy(b => b.CheckIn.Value.Date) // Group by actual Date
                        .Select(g => new
                        {
                            Date = g.Key,
                            Revenue = g.Sum(b => b.TotalPrice ?? 0)
                        })
                        .ToListAsync();

                    var revenueDictDay = revenueByDay.ToDictionary(r => r.Date, r => r.Revenue);

                    // 2. 產生日期區間內的所有日期
                    var allDaysInRange = new List<DateTime>();
                    var loopDateDay = req.StartDate.Date;
                    var finalDateDay = req.EndDate.Date;

                    while (loopDateDay <= finalDateDay)
                    {
                        allDaysInRange.Add(loopDateDay);
                        loopDateDay = loopDateDay.AddDays(1);
                    }

                    // 3. 合併資料，沒有收入的日期補 0
                    var fullDailyReport = allDaysInRange.Select(day => new
                    {
                        date = day.ToString("yyyy-MM-dd"),
                        revenue = revenueDictDay.TryGetValue(day, out var revenue) ? revenue : 0
                    });

                    return Ok(fullDailyReport.OrderBy(r => r.date));
            }
        }

        [HttpPost("GetRevenueKpi")]
        public async Task<IActionResult> GetRevenueKpi([FromBody] RevenueKpiRequestDto req)
        {
            // TODO: 之後需從登入資訊取得 HostId
            int hostId = 47;

            List<int> roomIdsToQuery;

            if (req.RoomIds == null || !req.RoomIds.Any())
            {
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId)
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }
            else
            {
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId && req.RoomIds.Contains(r.RoomId))
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }

            if (!roomIdsToQuery.Any())
            {
                return Ok(new { totalRevenue = 0 });
            }

            var startDate = DateTime.Today.AddDays(-req.Days);
            var endDate = DateTime.Today;

            var totalRevenue = await _context.Bookings
                .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                .Where(b => b.Status == "Completed" || b.Status == "Confirmed")
                .Where(b => b.CheckIn.HasValue && b.CheckIn.Value.Date >= startDate && b.CheckIn.Value.Date <= endDate)
                .SumAsync(b => b.TotalPrice ?? 0);

            return Ok(new { totalRevenue });
        }

        [HttpPost("GetRevenueSourceAnalysis")]
        public async Task<IActionResult> GetRevenueSourceAnalysis([FromBody] AnalysisRequestDto req)
        {
            // TODO: 之後需從登入資訊取得 HostId
            int hostId = 47;

            List<int> roomIdsToQuery;

            if (req.RoomIds == null || !req.RoomIds.Any())
            {
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId)
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }
            else
            {
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId && req.RoomIds.Contains(r.RoomId))
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }

            if (!roomIdsToQuery.Any())
            {
                return Ok(new List<RevenueSourceDataPoint>());
            }

            var startDate = DateTime.Today.AddDays(-req.Days);
            var endDate = DateTime.Today;

            var analysis = await _context.Bookings
                .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                .Where(b => b.Status == "Completed" || b.Status == "Confirmed")
                .Where(b => b.CheckIn.HasValue && b.CheckIn.Value.Date >= startDate && b.CheckIn.Value.Date <= endDate)
                .GroupBy(b => new { b.RoomId, b.Room.Title })
                .Select(g => new RevenueSourceDataPoint
                {
                    RoomId = g.Key.RoomId.Value,
                    RoomTitle = g.Key.Title,
                    TotalRevenue = g.Sum(b => b.TotalPrice ?? 0)
                })
                .Where(r => r.TotalRevenue > 0) // Only include rooms with revenue
                .OrderByDescending(r => r.TotalRevenue)
                .ToListAsync();

            return Ok(analysis);
        }

        [HttpPost("GetRevenuePrediction")]
        public async Task<IActionResult> GetRevenuePrediction([FromBody] PredictionRequestDto req)
        {
            const int historicalDays = 90;
            var today = DateTime.Today;
            var historicalStartDate = today.AddDays(-historicalDays);

            var roomIdsToQuery = await GetRoomIdsToQuery(req.RoomIds);
            if (!roomIdsToQuery.Any())
            {
                return Ok(new RevenuePredictionResponseDto { HistoricalPoints = new List<RevenuePoint>(), RegressionPoints = new List<RevenuePoint>() });
            }

            // 1. Fetch historical data
            var historicalQuery = _context.Bookings
                .Where(b => b.Status == "Completed" || b.Status == "Confirmed")
                .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                .Where(b => b.CheckIn.HasValue && b.CheckIn.Value.Date >= historicalStartDate && b.CheckIn.Value.Date < today);

            var revenueByDay = await historicalQuery
                .GroupBy(b => b.CheckIn.Value.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(b => b.TotalPrice ?? 0) })
                .ToDictionaryAsync(r => r.Date, r => r.Revenue);

            var historicalPoints = Enumerable.Range(0, historicalDays)
                .Select(i => historicalStartDate.AddDays(i))
                .Select(day => new RevenuePoint
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    Revenue = revenueByDay.TryGetValue(day, out var revenue) ? revenue : 0
                }).ToList();

            // 2. Simple Linear Regression for the entire period
            var regressionPoints = new List<RevenuePoint>();
            if (historicalPoints.Count > 7) // Need enough data to predict
            {
                // Use last 30 days for a more stable trend calculation
                var trendData = historicalPoints.TakeLast(30).ToList();
                var n = trendData.Count;
                var sumX = Enumerable.Range(1, n).Sum(i => (long)i); // Use long to avoid overflow
                var sumY = trendData.Sum(p => (double)p.Revenue);
                var sumXY = Enumerable.Range(1, n).Sum(i => (long)i * (double)trendData[i - 1].Revenue);
                var sumX2 = Enumerable.Range(1, n).Sum(i => (long)i * i);

                var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
                var intercept = (sumY - slope * sumX) / n;

                var totalDays = historicalDays + req.ForecastDays;
                for (int i = 0; i < totalDays; i++)
                {
                    var date = historicalStartDate.AddDays(i);
                    // Adjust index for prediction calculation, relative to the start of trend data
                    var predictionIndex = (date - DateTime.Parse(trendData.First().Date)).Days + 1;
                    var regressionRevenue = (decimal)(slope * predictionIndex + intercept);
                    if (regressionRevenue < 0) regressionRevenue = 0;

                    regressionPoints.Add(new RevenuePoint
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        Revenue = regressionRevenue
                    });
                }
            }

            return Ok(new RevenuePredictionResponseDto
            {
                HistoricalPoints = historicalPoints,
                RegressionPoints = regressionPoints
            });
        }

        private async Task<List<int>> GetRoomIdsToQuery(List<int> requestedRoomIds)
        {
            // TODO: Replace with actual host ID from user context
            int hostId = 47;

            var hostRoomsQuery = _context.RoomLists.Where(r => r.HostId == hostId);

            if (requestedRoomIds != null && requestedRoomIds.Any())
            {
                hostRoomsQuery = hostRoomsQuery.Where(r => requestedRoomIds.Contains(r.RoomId));
            }

            return await hostRoomsQuery.Select(r => r.RoomId).ToListAsync();
        }
    }
}
