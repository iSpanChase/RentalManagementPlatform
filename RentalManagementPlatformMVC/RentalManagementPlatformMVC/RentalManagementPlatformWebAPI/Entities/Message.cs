namespace RentalManagementPlatformWebAPI.Entities
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TicketId { get; set; }
        public string SenderRole { get; set; } = default!;  // user / agent
        public string SenderName { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
