using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services.Interfaces;

public interface IMessageService
{
    IEnumerable<MessageDto> GetByTicketId(Guid ticketId);
    MessageDto Save(Guid ticketId, string senderRole, string senderName, string content);
}
