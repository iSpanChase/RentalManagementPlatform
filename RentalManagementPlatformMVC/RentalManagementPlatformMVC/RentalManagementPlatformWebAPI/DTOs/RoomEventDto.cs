using System;

namespace RentalManagementPlatformWebAPI.DTOs
{
    public class RoomEventDto
    {
        public int RoomId { get; set; }
        public RoomEventType EventType { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? TriggeredBy { get; set; } // e.g., User ID or system
    }
}
