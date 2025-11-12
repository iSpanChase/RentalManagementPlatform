using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RentalManagementPlatformWebAPI.Services.AI.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [ApiController]
    [Route("api/ai/[controller]")]
    public class AISearchController : ControllerBase
    {
        private readonly INaturalLanguageSearchService _aiSearchService;
        private readonly ILogger<AISearchController> _logger;

        public AISearchController(
            INaturalLanguageSearchService aiSearchService,
            ILogger<AISearchController> logger)
        {
            _aiSearchService = aiSearchService;
            _logger = logger;
        }

        /// <summary>
        /// AI 自然語言房源搜尋
        /// </summary>
        /// <param name="query">自然語言查詢，例如：「幫我查詢101附近五公里的房源」</param>
        /// <returns>搜尋結果</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "查詢語句不能為空" });
            }

            try
            {
                _logger.LogInformation("AI搜尋請求: {Query}", query);
                
                var results = await _aiSearchService.ProcessNaturalLanguageSearchAsync(query);
                
                _logger.LogInformation("AI搜尋完成，找到 {Count} 筆結果", results.Count());
                
                return Ok(new
                {
                    success = true,
                    query = query,
                    results = results,
                    count = results.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI搜尋處理失敗: {Query}", query);
                return StatusCode(500, new { error = "搜尋處理失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 解析自然語言查詢（調試用）
        /// </summary>
        /// <param name="query">自然語言查詢</param>
        /// <returns>解析後的參數</returns>
        [HttpGet("parse")]
        [AllowAnonymous]
        public async Task<IActionResult> ParseQuery([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "查詢語句不能為空" });
            }

            try
            {
                var parameters = await _aiSearchService.ParseNaturalLanguageQueryAsync(query);
                
                return Ok(new
                {
                    success = true,
                    originalQuery = query,
                    parsedParameters = parameters
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢解析失敗: {Query}", query);
                return StatusCode(500, new { error = "解析失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 取得支援的範例查詢
        /// </summary>
        /// <returns>範例查詢列表</returns>
        [HttpGet("examples")]
        [AllowAnonymous]
        public IActionResult GetExamples()
        {
            var examples = new[]
            {
                new { query = "幫我查詢101附近五公里的房源", description = "地標附近搜尋" },
                new { query = "找台北車站附近3公里$1000-2000的房間", description = "地標+價格範圍" },
                new { query = "搜尋大安區2人住的房源", description = "行政區+人數" },
                new { query = "幫我找信義區活躍的房源", description = "行政區+狀態" },
                new { query = "查詢25.033,121.565附近10公里的住宿", description = "座標附近搜尋" },
                new { query = "找台北雙人房$1500以內", description = "城市+人數+價格" },
                new { query = "搜尋板橋車站附近可預訂的房間", description = "地標+狀態" }
            };

            return Ok(new
            {
                success = true,
                examples = examples
            });
        }
    }
}