using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Recommendation;
using RentalManagementPlatformWebAPI.Area.ReportForm.Services;
using ChaseCheng.Global.Utilities.Extension;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;

        public RecommendationController(RecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpPost("ForGuest")]
        public async Task<ActionResult<IEnumerable<RecommendedRoomDto>>> GetGuestRecommendations(
            [FromBody] RecommendationRequestDto request)
        {
            // 1. 呼叫 Service，從快取或計算結果中獲取 Top N 筆推薦
            var topNRecommendations = await _recommendationService.GetRecommendationsForGuest(request.GuestId, request.TopN);

            if (topNRecommendations == null || !topNRecommendations.Any())
            {
                return Ok(new List<RecommendedRoomDto>()); // 如果沒有推薦結果，回傳空列表
            }

            // 2. 確保 displayM 不大於實際推薦數量
            int finalDisplayCount = Math.Min(request.DisplayM, topNRecommendations.Count);

            // 3. 從 Top N 中隨機抽取 M 筆
            var randomMRecommendations = topNRecommendations.GetRandomUnique(finalDisplayCount);

            // 4. 回傳最終的隨機結果
            return Ok(randomMRecommendations);
        }
    }
}
