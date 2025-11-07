namespace RentalManagementPlatformWebAPI.Entities
{
    public class SupportTicket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Status { get; set; } = "open"; // open / active / closed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int RequesterId { get; set; }
    }
}
