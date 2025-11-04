
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ApiControllerBase
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public RoomsController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        [HttpGet("ForHost")]
        public async Task<IActionResult> GetRoomsForHost()
        {
            int hostId = CurrentUserId;

            var rooms = await _context.RoomLists
                .Where(r => r.HostId == hostId && !r.IsDeleted)
                .Select(r => new { r.RoomId, r.Title })
                .ToListAsync();

            return Ok(rooms);
        }
    }
}
