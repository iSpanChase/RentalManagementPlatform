using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services.Interfaces;

public interface ISupportTicketService
{
    TicketDto Create(string title, string userName);
    IEnumerable<TicketDto> List(string? status);
    bool Close(Guid id);
    bool Exists(Guid id);
}