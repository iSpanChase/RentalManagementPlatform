using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs; // Assuming RoomListSearchDto is here
using RentalManagementPlatformWebAPI.Services; // Assuming MeilisearchService is here
using Microsoft.AspNetCore.Authorization;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // Apply authorization if needed
    public class SearchController : ControllerBase
    {
        private readonly MeilisearchService _meilisearchService;

        public SearchController(MeilisearchService meilisearchService)
        {
            _meilisearchService = meilisearchService;
        }

        // GET: api/Search?query=xxx&status=yyy
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomListSearchDto>>> SearchRooms(string? query, string? status)
        {
            var q = (query ?? string.Empty).Trim();
            var s = (status ?? string.Empty).Trim();

            var hits = await _meilisearchService.SearchAsync(q, s);
            return Ok(hits);
        }
    }
}