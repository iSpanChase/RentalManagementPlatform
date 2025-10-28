using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO;
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
    }
}
