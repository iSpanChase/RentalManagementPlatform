using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketsController : ControllerBase
    {
        private readonly ISupportTicketService _svc;
        public SupportTicketsController(ISupportTicketService svc) => _svc = svc;

        [HttpPost]
        public ActionResult<TicketDto> Create([FromBody] CreateTicketReq req)
        {
            try
            {
                var uidStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(uidStr, out var uid)) return Unauthorized();
                var dto = _svc.Create(req.Title, req.UserName, uid);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 先暫時輸出詳細錯誤（只在開發環境）
                Console.WriteLine(ex);
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<TicketDto>> List([FromQuery] string? status)
            => Ok(_svc.List(status));

        [HttpPut("{id:guid}/close")]
        public IActionResult Close([FromRoute] Guid id)
            => _svc.Close(id) ? NoContent() : NotFound();
    }
}
