using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Services.Assistant;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssistantController : ControllerBase
    {
        private readonly IPlacesService _places;
        private readonly MeilisearchService _meilisearch;
        private readonly AssistantOrchestrator _orchestrator;

        public AssistantController(IPlacesService places, MeilisearchService meilisearch, AssistantOrchestrator orchestrator)
        {
            _places = places;
            _meilisearch = meilisearch;
            _orchestrator = orchestrator;
        }

        public record PlaceSearchRequest(string q);
        public record RoomsNearbyRequest(double lat, double lng, double radiusKm, string? query = null, string? status = null, bool sortByDistance = true);
        public record ChatRequest(string text);

        [HttpGet("tools")]
        [AllowAnonymous]
        public IActionResult GetTools()
        {
            var tools = new object[]
            {
                new {
                    type = "function",
                    function = new {
                        name = "places_search",
                        description = "唯讀。將地點或地標名稱（例如：台北101）轉換為座標。當用戶提供地點名稱而非經緯度時使用此函數。返回最多 5 個最佳候選結果。",
                        parameters = new {
                            type = "object",
                            properties = new {
                                q = new { type = "string", description = "地點或地標名稱（任何語言）。" }
                            },
                            required = new [] { "q" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "search_rooms_nearby",
                        description = "唯讀。在指定的經緯度周圍半徑範圍內（公里）搜尋房源。若用戶提供地點名稱，請先呼叫 places_search。當 sortByDistance=true 時按距離排序。",
                        parameters = new {
                            type = "object",
                            properties = new {
                                lat = new { type = "number", description = "緯度（WGS84 坐標系，範圍 -90 至 90）。" },
                                lng = new { type = "number", description = "經度（WGS84 坐標系，範圍 -180 至 180）。" },
                                radiusKm = new { type = "number", description = "搜尋半徑，單位為公里。", minimum = 0.5, maximum = 50, _default = 5 },
                                query = new { type = "string", description = "選擇性的關鍵字篩選條件（例如：房源標題關鍵詞）。" },
                                status = new { type = "string", description = "房源狀態篩選條件。", _enum = new [] { "Active" }, _default = "Active" },
                                sortByDistance = new { type = "boolean", description = "若為 true，則按距離排序結果。", _default = true }
                            },
                            required = new [] { "lat", "lng", "radiusKm" }
                        }
                    }
                }
            };

            return Ok(tools);
        }

        [HttpPost("places_search")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<PlaceSuggestionDto>>> PlaceSearch([FromBody] PlaceSearchRequest req, CancellationToken ct)
            => Ok(await _places.SearchAsync(req.q, 5, ct));

        [HttpPost("search_rooms_nearby")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoomListSearchDto>>> SearchRoomsNearby([FromBody] RoomsNearbyRequest req, CancellationToken ct)
        {
            var q = (req.query ?? string.Empty).Trim();
            var s = (req.status ?? string.Empty).Trim();
            var hits = await _meilisearch.SearchNearbyAsync(q, req.lat, req.lng, req.radiusKm, s, req.sortByDistance);
            return Ok(hits);
        }

        [HttpPost("chat")]
        [AllowAnonymous]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req, CancellationToken ct)
        {
            var (message, data) = await _orchestrator.ChatAsync(req.text, ct);
            return Ok(new { message, data });
        }
    }
}
