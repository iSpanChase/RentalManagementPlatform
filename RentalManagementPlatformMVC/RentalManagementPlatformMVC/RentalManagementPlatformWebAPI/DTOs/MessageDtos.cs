namespace RentalManagementPlatformWebAPI.DTOs
{
    public record MessageDto(Guid Id, Guid TicketId, string SenderRole, string SenderName, string Content, DateTime CreatedAt);
}
