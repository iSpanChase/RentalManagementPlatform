using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interface;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _svc;
        public MessagesController(IMessageService svc) => _svc = svc;

        [HttpGet("{ticketId:guid}")]
        public ActionResult<IEnumerable<MessageDto>> Get([FromRoute] Guid ticketId)
            => Ok(_svc.GetByTicketId(ticketId));
    }
}
