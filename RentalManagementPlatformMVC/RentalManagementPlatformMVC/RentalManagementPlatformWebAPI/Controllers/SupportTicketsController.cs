using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketsController : ControllerBase
    {
        private readonly ISupportTicketService _svc;
        public SupportTicketsController(ISupportTicketService svc) => _svc = svc;

        [HttpPost]
        public ActionResult<TicketDto> Create(CreateTicketReq req)
            => Ok(_svc.Create(req.Title, req.UserName));

        [HttpGet]
        public ActionResult<IEnumerable<TicketDto>> List([FromQuery] string? status)
            => Ok(_svc.List(status));

        [HttpPut("{id:guid}/close")]
        public IActionResult Close([FromRoute] Guid id)
            => _svc.Close(id) ? NoContent() : NotFound();
    }
}
