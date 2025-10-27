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
            var query = _context.Bookings
                .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                .Where(b => b.CheckIn.HasValue && b.CheckIn.Value.Date >= req.StartDate.Date && b.CheckIn.Value.Date <= req.EndDate.Date);

            // 根據 GroupBy 進行分組加總
            var result = query
                .AsEnumerable() // Switch to client-side evaluation for GroupBy
                .GroupBy(b =>
                {
                    switch (req.GroupBy?.ToLower())
                    {
                        case "month":
                            return b.CheckIn.Value.ToString("yyyy-MM-01");
                        case "week":
                            // 使用 ISO 8601 標準取得週數，並計算出該週的星期一作為代表日
                            var checkInDate = b.CheckIn.Value.Date;
                            int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                checkInDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                            var firstDayOfYear = new DateTime(checkInDate.Year, 1, 1);
                            var firstDayOfGivenWeek = firstDayOfYear.AddDays((weekOfYear - 1) * 7 - (int)firstDayOfYear.DayOfWeek + (int)DayOfWeek.Monday);
                            return firstDayOfGivenWeek.ToString("yyyy-MM-dd");
                        case "day":
                        default:
                            return b.CheckIn.Value.ToString("yyyy-MM-dd");
                    }
                })
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.TotalPrice ?? 0)
                })
                .OrderBy(r => r.Date);

            return Ok(result);
        }
    }
}
