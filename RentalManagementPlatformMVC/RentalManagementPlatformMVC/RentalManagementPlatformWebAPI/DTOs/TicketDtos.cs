namespace RentalManagementPlatformWebAPI.DTOs
{
    public record CreateTicketReq(string Title, string UserName);
    public record TicketDto(Guid Id, string Title, string UserName, string Status, DateTime CreatedAt, DateTime UpdatedAt);
}
