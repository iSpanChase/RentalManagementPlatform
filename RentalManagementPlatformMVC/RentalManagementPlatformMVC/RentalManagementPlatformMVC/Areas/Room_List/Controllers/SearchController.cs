using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RentalManagementPlatformMVC.Areas.Room_List.Controllers
{
    [Area("Room_List")]
    [Route("Room_List/[controller]")]
    [Authorize]
    public class SearchController : Controller
    {
        private readonly MeilisearchService _meilisearchService;

        public SearchController(MeilisearchService meilisearchService)
        {
            _meilisearchService = meilisearchService;
        }

        // 首頁：只回殼（View），實際搜尋走 AJAX
        [HttpGet("")]
        public IActionResult Index(string? query)
        {
            ViewData["CurrentQuery"] = query ?? string.Empty;
            return View(); // 不預載資料，交給前端 AJAX
        }

        // AJAX Partial View 端點：GET /Room_List/Search/Ajax?query=xxx
        [HttpGet("Ajax")]
        public async Task<IActionResult> Ajax(string? query,string ?status)
        {
            var q = (query ?? string.Empty).Trim();
            var s = (status ?? string.Empty).Trim();
            // 即使查詢為空，也執行搜尋，服務層應能處理空查詢並返回空列表
            var hits = await _meilisearchService.SearchAsync(q,s); 
            // 將結果傳遞給 Partial View
            return PartialView("_SearchResultsPartial", hits);
        }
    }
}