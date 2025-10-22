using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        [HttpPost("GetRevenue")]
        public async Task<IActionResult> GetRevenue([FromBody] RevenueRequestDto req)
        {
            // 模擬資料
            var data = new[]
            {
                new { Date = "2025-10-01", Revenue = 1200 },
                new { Date = "2025-10-02", Revenue = 1500 },
                new { Date = "2025-10-03", Revenue = 1000 },
            };
            return Ok(data);
        }
    }
}
